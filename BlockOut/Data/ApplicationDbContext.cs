using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BlockOut.Models;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Schedule> Schedules { get; set; } // Registers the Schedule model with the database context
    public DbSet<Business> Businesses { get; set; } // Register Business model with DbContext
}