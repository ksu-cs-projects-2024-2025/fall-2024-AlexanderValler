using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BlockOut.Data;
using BlockOut.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace BlockOut.Pages.Businesses
{
    [Authorize]
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
        public string? ConflictingBusinessNameMessage { get; set; } = string.Empty;

        public string[] DaysOfWeek = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

        public async Task<IActionResult> OnGetAsync()
        {
            // Initialize OpenHours with default values
            Business.OpenHours = Enumerable.Range(1, 7).Select(day => new OpenHours
            {
                Day = day, // Assign the correct day number
                OpenTime = new TimeSpan(9, 0, 0),
                CloseTime = new TimeSpan(17, 0, 0),
                IsClosed = false
            }).ToList();

            ConflictingBusinessNameMessage = string.Empty;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Ensure a unique Business.Id is generated
            if (string.IsNullOrEmpty(Business.Id))
            {
                Business.Id = GenerateUniqueBusinessId();
            }
            ConflictingBusinessNameMessage ??= string.Empty;

            ModelState.Remove("Business.Id");
            ModelState.Remove("ConflictingBusinessNameMessage");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Assign the correct `Day` values to `OpenHours`
            for (int i = 0; i < Business.OpenHours.Count; i++)
            {
                Business.OpenHours[i].Day = i + 1; // Day starts from 1 (Sunday) to 7 (Saturday)
                Business.OpenHours[i].BusinessId = Business.Id; // Ensure BusinessId is assigned
            }

            // Fetch the current user and add as owner
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Index");
            }

            Business.UserBusinessRoles = new List<UserBusinessRole>
            {
                new UserBusinessRole
                {
                    UserId = user.Id,
                    BusinessId = Business.Id,
                    Role = "Owner"
                }
            };

            try
            {
                // Save the Business and associated OpenHours
                _context.Businesses.Add(Business);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return Page();
            }

            return RedirectToPage("/Dashboard");
        }

        public JsonResult OnGetValidateBusinessName(string name)
        {
            string normalizedName = NormalizeBusinessName(name);
            bool conflict = _context.Businesses.AsEnumerable()
                .Any(b => AreNamesTooSimilar(NormalizeBusinessName(b.Name), normalizedName));
            return new JsonResult(new { conflict });
        }

        private string NormalizeBusinessName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }

            return Regex.Replace(name.ToUpperInvariant(), @"[^A-Z]", string.Empty);
        }

        private bool AreNamesTooSimilar(string existingName, string newName)
        {
            int distance = CalculateLevenshteinDistance(existingName, newName);
            double similarity = 1.0 - (double)distance / Math.Max(existingName.Length, newName.Length);

            return similarity >= 0.8 && distance <= 3;
        }

        private int CalculateLevenshteinDistance(string s, string t)
        {
            int[,] dp = new int[s.Length + 1, t.Length + 1];
            for (int i = 0; i <= s.Length; i++) dp[i, 0] = i;
            for (int j = 0; j <= t.Length; j++) dp[0, j] = j;

            for (int i = 1; i <= s.Length; i++)
            {
                for (int j = 1; j <= t.Length; j++)
                {
                    int cost = s[i - 1] == t[j - 1] ? 0 : 1;
                    dp[i, j] = Math.Min(dp[i - 1, j] + 1, Math.Min(dp[i, j - 1] + 1, dp[i - 1, j - 1] + cost));
                }
            }

            return dp[s.Length, t.Length];
        }

        private string GenerateUniqueBusinessId()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[new Random().Next(s.Length)]).ToArray());
        }
    }
}
