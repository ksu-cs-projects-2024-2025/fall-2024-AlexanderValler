// File: Data/SeedData.cs

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

            // Ensure each role exists in the database
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

                // Create "Test Business" if it doesn't exist
                var business = context.Businesses.FirstOrDefault(b => b.Name == "Test Business");
                if (business == null)
                {
                    business = new Business
                    {
                        Name = "Test Business",
                        OpenHours = new List<OpenHours>
                        {
                            new OpenHours { Day = "Sunday", OpenTime = new TimeSpan(9, 0, 0), CloseTime = new TimeSpan(17, 0, 0) },
                            new OpenHours { Day = "Monday", OpenTime = new TimeSpan(9, 0, 0), CloseTime = new TimeSpan(17, 0, 0) },
                            new OpenHours { Day = "Tuesday", OpenTime = new TimeSpan(9, 0, 0), CloseTime = new TimeSpan(17, 0, 0) },
                            new OpenHours { Day = "Wednesday", OpenTime = new TimeSpan(9, 0, 0), CloseTime = new TimeSpan(17, 0, 0) },
                            new OpenHours { Day = "Thursday", OpenTime = new TimeSpan(9, 0, 0), CloseTime = new TimeSpan(17, 0, 0) },
                            new OpenHours { Day = "Friday", OpenTime = new TimeSpan(9, 0, 0), CloseTime = new TimeSpan(17, 0, 0) },
                            new OpenHours { Day = "Saturday", OpenTime = new TimeSpan(9, 0, 0), CloseTime = new TimeSpan(17, 0, 0) },
                        }
                    };
                    context.Businesses.Add(business);
                    await context.SaveChangesAsync();
                }

                // Create initial users and assign business roles
                var ownerUser = await CreateUserIfNotExists(userManager, "TestOwner", "owner@example.com", "OwnerPassword123!", 2, "CEO");
                var managerUser = await CreateUserIfNotExists(userManager, "TestManager", "manager@example.com", "ManagerPassword123!", 1, "Project Manager");
                var employeeUser = await CreateUserIfNotExists(userManager, "TestEmployee", "employee@example.com", "EmployeePassword123!", 3, "Team Member");

                // Assign roles in the business
                await AssignBusinessRole(context, ownerUser, business, "Owner");
                await AssignBusinessRole(context, managerUser, business, "Manager");
                await AssignBusinessRole(context, employeeUser, business, "Employee");
            }
        }

        private static async Task<ApplicationUser> CreateUserIfNotExists(UserManager<ApplicationUser> userManager, string userName, string email, string password, int profilePictureId, string jobTitle)
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
                    AvailabilityCalendar = new Calendar { Name = "AvailabilityCalendar", Type = "Weekly", Data = "{}" },
                    PreferencesCalendar = new Calendar { Name = "PreferencesCalendar", Type = "Weekly", Data = "{}" }
                };

                var result = await userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    throw new Exception($"Failed to create user {userName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
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
