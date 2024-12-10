using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BlockOut.Models;

namespace BlockOut.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Ensure that SQLite foreign key constraints are enabled
            optionsBuilder.UseSqlite("Data Source=app.db;");
            optionsBuilder.EnableSensitiveDataLogging(); // Optional: Enables detailed logging (for debugging)
        }

        // Custom entities
        public DbSet<Business> Businesses { get; set; }
        public DbSet<Calendar> Calendars { get; set; }
        public DbSet<OpenHours> OpenHours { get; set; }
        public DbSet<UserBusinessRole> UserBusinessRoles { get; set; } // For multi-role support
        public DbSet<UserBusinessCalendar> UserBusinessCalendars { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<ShiftHourRequirement> ShiftHourRequirements { get; set; }
        public DbSet<ScheduleEntry> ScheduleEntries { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Required to configure ASP.NET Identity tables

            // Configure AvailabilityCalendar relationship
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.AvailabilityCalendar)
                .WithOne(c => c.AvailabilityUser)
                .HasForeignKey<ApplicationUser>(u => u.AvailabilityCalendarId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure PreferencesCalendar relationship
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.PreferencesCalendar)
                .WithOne(c => c.PreferencesUser)
                .HasForeignKey<ApplicationUser>(u => u.PreferencesCalendarId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure many-to-many relationship for UserBusinessRole
            modelBuilder.Entity<UserBusinessRole>()
                .HasKey(ubr => new { ubr.UserId, ubr.BusinessId }); // Composite key for uniqueness

            modelBuilder.Entity<UserBusinessRole>()
                .HasOne(ubr => ubr.User)
                .WithMany(u => u.UserBusinessRoles)
                .HasForeignKey(ubr => ubr.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserBusinessRole>()
                .HasOne(ubr => ubr.Business)
                .WithMany(b => b.UserBusinessRoles)
                .HasForeignKey(ubr => ubr.BusinessId)
                .OnDelete(DeleteBehavior.Cascade);

            // Add unique composite index for UserBusinessRole
            modelBuilder.Entity<UserBusinessRole>()
                .HasIndex(ubr => new { ubr.UserId, ubr.BusinessId })
                .IsUnique();

            // Configure one-to-many relationship between Business and Calendars
            modelBuilder.Entity<Business>()
                .HasMany(b => b.Calendars)
                .WithOne(c => c.Business)
                .HasForeignKey(c => c.BusinessId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure one-to-many relationship between Business and OpenHours
            modelBuilder.Entity<Business>()
                .HasMany(b => b.OpenHours)
                .WithOne(oh => oh.Business)
                .HasForeignKey(oh => oh.BusinessId)
                .OnDelete(DeleteBehavior.Cascade);

            // Ensure the Calendar side reflects the same relationship
            modelBuilder.Entity<Calendar>()
                .HasOne(c => c.AvailabilityUser)
                .WithOne(u => u.AvailabilityCalendar)
                .HasForeignKey<Calendar>(c => c.AvailabilityUserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Calendar>()
                .HasOne(c => c.PreferencesUser)
                .WithOne(u => u.PreferencesCalendar)
                .HasForeignKey<Calendar>(c => c.PreferencesUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Composite key for UserBusinessCalendar
            modelBuilder.Entity<UserBusinessCalendar>()
                .HasKey(ubc => new { ubc.UserId, ubc.CalendarId, ubc.BusinessId });

            modelBuilder.Entity<UserBusinessCalendar>()
                .HasOne(ubc => ubc.User)
                .WithMany(u => u.UserBusinessCalendars)
                .HasForeignKey(ubc => ubc.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserBusinessCalendar>()
                .HasOne(ubc => ubc.Calendar)
                .WithMany(c => c.UserBusinessCalendars)
                .HasForeignKey(ubc => ubc.CalendarId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserBusinessCalendar>()
                .HasOne(ubc => ubc.Business)
                .WithMany(b => b.UserBusinessCalendars)
                .HasForeignKey(ubc => ubc.BusinessId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Shift relationships
            modelBuilder.Entity<Shift>()
                .HasOne(s => s.Business)
                .WithMany(b => b.Shifts)
                .HasForeignKey(s => s.BusinessId)
                .OnDelete(DeleteBehavior.Cascade); // Ensures shifts are deleted when the associated business is deleted

            modelBuilder.Entity<Shift>()
                .HasOne(s => s.Calendar)
                .WithMany() // Assuming calendars don’t need a navigation property back to shifts
                .HasForeignKey(s => s.CalendarId)
                .OnDelete(DeleteBehavior.SetNull); // Retains shifts if the calendar is deleted (optional)

            // Configure ShiftHourRequirement relationship
            modelBuilder.Entity<ShiftHourRequirement>()
                .HasOne(hr => hr.Shift)
                .WithMany(s => s.HourlyRequirements)
                .HasForeignKey(hr => hr.ShiftId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure composite keys or additional indexes as necessary
            modelBuilder.Entity<UserBusinessRole>()
                .HasKey(ubr => new { ubr.UserId, ubr.BusinessId });

            modelBuilder.Entity<UserBusinessCalendar>()
                .HasKey(ubc => new { ubc.UserId, ubc.CalendarId, ubc.BusinessId });
        }
    }
}
