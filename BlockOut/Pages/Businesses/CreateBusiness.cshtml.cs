using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BlockOut.Data;
using BlockOut.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlockOut;

namespace BlockOut.Pages.Businesses
{
    public class CreateBusinessModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CreateBusinessModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public Business Business { get; set; } = new Business();

        [BindProperty]
        public bool BusinessNameExists { get; set; } = false;

        [BindProperty]
        public string UserRole { get; set; } = "Owner"; // Default to "Owner"


        public string[] DaysOfWeek = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            string normalizedBusinessName = new string(Business.Name.Where(char.IsLetterOrDigit).ToArray()).ToLower();

            BusinessNameExists = _context.Businesses.Any(b => new string(b.Name.Where(char.IsLetterOrDigit).ToArray()).ToLower() == normalizedBusinessName);
            if (BusinessNameExists)
            {
                return Page();
            }

            // Set open hours for each day
            foreach (var day in DaysOfWeek)
            {
                Business.OpenHours.Add(new OpenHours
                {
                    Day = day,
                    OpenTime = Business.OpenHours.FirstOrDefault(oh => oh.Day == day)?.OpenTime ?? new TimeSpan(9, 0, 0), // Default to 9 AM
                    CloseTime = Business.OpenHours.FirstOrDefault(oh => oh.Day == day)?.CloseTime ?? new TimeSpan(17, 0, 0) // Default to 5 PM
                });
            }

            var user = await _userManager.GetUserAsync(User);
            Business.UserBusinessRoles.Add(new UserBusinessRole
            {
                UserId = user.Id,
                Business = Business,
                Role = "Owner"
            });


            _context.Businesses.Add(Business);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Businesses/Index");
        }
    }
}
