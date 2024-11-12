using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BlockOut.Models;

namespace BlockOut.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // Custom entities
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Business> Businesses { get; set; }
        public DbSet<Calendar> Calendars { get; set; }
        public DbSet<OpenHours> OpenHours { get; set; }
        public DbSet<UserBusinessRole> UserBusinessRoles { get; set; } // For multi-role support

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Required to configure ASP.NET Identity tables

            // Configure the AvailabilityCalendar and PreferencesCalendar relationships for ApplicationUser
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.AvailabilityCalendar)
                .WithOne(c => c.AvailabilityUser)
                .HasForeignKey<Calendar>(c => c.AvailabilityUserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.PreferencesCalendar)
                .WithOne(c => c.PreferencesUser)
                .HasForeignKey<Calendar>(c => c.PreferencesUserId)
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
        }
    }
}
