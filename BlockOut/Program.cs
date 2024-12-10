using BlockOut.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BlockOut.Models;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);


        // Add services to the container
        builder.Services.AddRazorPages();

        // Add Entity Framework Core and Identity with ApplicationUser and roles
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        // Configure the application cookie to redirect unauthenticated users
        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/"; // Redirect to the homepage if the user is not logged in
            options.AccessDeniedPath = "/"; // Redirect to the homepage if access is denied
        });

        // Add session services
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout
            options.Cookie.HttpOnly = true; // For security
            options.Cookie.IsEssential = true; // Required if GDPR compliance is needed
        });

        var app = builder.Build();

        // Apply migrations and seed data
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                // Ensure database migrations are applied
                var context = services.GetRequiredService<ApplicationDbContext>();
                await context.Database.MigrateAsync(); // This ensures migrations are applied

                // Seed roles and users
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                await SeedData.Initialize(services, userManager, roleManager);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during database migration or seeding: {ex.Message}");
            }
        }

        // Configure the HTTP request pipeline
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts(); // Only applies HSTS in non-development mode
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        // Use session middleware
        app.UseSession();

        app.MapRazorPages();

        app.Run();
    }
}
