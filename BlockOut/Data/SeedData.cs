using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using BlockOut.Models;

namespace BlockOut.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Define role names
            string[] roleNames = { "Owner", "Manager", "Employee" };

            // Ensure roles exist
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            using (var context = serviceProvider.GetRequiredService<ApplicationDbContext>())
            {
                // Ensure database is created
                context.Database.EnsureCreated();

                // Seed Business
                var business = await SeedBusinessAsync(context);

                // Seed Users
                var owner = await SeedUserAsync(userManager, context, "TestOwner", "owner@example.com", "OwnerPassword123!", 2, "CEO");
                var manager = await SeedUserAsync(userManager, context, "TestManager", "manager@example.com", "ManagerPassword123!", 1, "Project Manager");
                var employee = await SeedUserAsync(userManager, context, "TestEmployee", "employee@example.com", "EmployeePassword123!", 3, "Team Member");

                // Assign roles
                await AssignBusinessRole(context, owner, business, "Owner");
                await AssignBusinessRole(context, manager, business, "Manager");
                await AssignBusinessRole(context, employee, business, "Employee");
            }
        }

        private static async Task<Business> SeedBusinessAsync(ApplicationDbContext context)
        {
            var business = context.Businesses.FirstOrDefault(b => b.Name == "Test Business");
            if (business == null)
            {
                business = new Business
                {
                    Id = Guid.NewGuid().ToString("N").Substring(0, 8),
                    Name = "Test Business",
                    OpenHours = Enumerable.Range(1, 7).Select(day => new OpenHours
                    {
                        Day = day,
                        OpenTime = new TimeSpan(9, 0, 0),
                        CloseTime = new TimeSpan(17, 0, 0),
                    }).ToList()
                };
                context.Businesses.Add(business);
                await context.SaveChangesAsync();

                // Create Default Calendar
                var defaultCalendar = new Calendar
                {
                    Name = "Default Calendar",
                    Type = "Weekly",
                    Data = "{}",
                    BusinessId = business.Id
                };
                context.Calendars.Add(defaultCalendar);
                await context.SaveChangesAsync();
            }
            return business;
        }

        private static async Task<ApplicationUser> SeedUserAsync(UserManager<ApplicationUser> userManager, ApplicationDbContext context, string userName, string email, string password, int profilePictureId, string jobTitle)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = userName,
                    Email = email,
                    ProfilePictureId = profilePictureId,
                    JobTitle = jobTitle,
                };

                var result = await userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    throw new Exception($"Failed to create user {userName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }

                // Reload user to ensure it's fully tracked
                user = await userManager.FindByEmailAsync(email);

                // Create calendars
                var availabilityCalendar = new Calendar
                {
                    Name = $"{userName} Availability",
                    Type = "Weekly",
                    Data = "{}"
                };
                var preferencesCalendar = new Calendar
                {
                    Name = $"{userName} Preferences",
                    Type = "Weekly",
                    Data = "{}"
                };
                context.Calendars.AddRange(availabilityCalendar, preferencesCalendar);
                await context.SaveChangesAsync();

                // Link calendars to user
                user.AvailabilityCalendarId = availabilityCalendar.Id;
                user.PreferencesCalendarId = preferencesCalendar.Id;

                // Update user
                await userManager.UpdateAsync(user);
            }
            return user;
        }

        private static async Task AssignBusinessRole(ApplicationDbContext context, ApplicationUser user, Business business, string role)
        {
            if (!context.UserBusinessRoles.Any(ubr => ubr.UserId == user.Id && ubr.BusinessId == business.Id && ubr.Role == role))
            {
                var userBusinessRole = new UserBusinessRole
                {
                    UserId = user.Id,
                    BusinessId = business.Id,
                    Role = role
                };
                context.UserBusinessRoles.Add(userBusinessRole);
                await context.SaveChangesAsync();
            }
        }
    }
}
