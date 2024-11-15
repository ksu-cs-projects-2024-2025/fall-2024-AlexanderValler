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
    [Authorize] // Ensures only authenticated users can access this page
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
        public string ConflictingBusinessNameMessage { get; set; } = string.Empty;

        [BindProperty]
        public string UserRole { get; set; } = "Owner"; // Default to "Owner"

        public string[] DaysOfWeek = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

        public async Task<IActionResult> OnGetAsync()
        {
            if (Business.OpenHours == null || Business.OpenHours.Count == 0)
            {
                Business.OpenHours = DaysOfWeek.Select(day => new OpenHours
                {
                    Day = day,
                    OpenTime = new TimeSpan(9, 0, 0), // Default to 9 AM
                    CloseTime = new TimeSpan(17, 0, 0) // Default to 5 PM
                }).ToList();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Console.WriteLine("OnPostAsync triggered"); // Debugging output
            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState is invalid.");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Error: {error.ErrorMessage}");
                }
                return Page();
            }
            Console.WriteLine("ModelState is valid. Proceeding with business creation...");

            string generatedId;
            do
            {
                generatedId = GenerateUniqueBusinessId();
            } while (_context.Businesses.Any(b => b.Id == generatedId));

            Console.WriteLine($"Generated unique ID: {generatedId}");
            Business.Id = generatedId;

            string normalizedBusinessName = NormalizeBusinessName(Business.Name);

            BusinessNameExists = _context.Businesses
                .AsEnumerable()
                .Any(b => AreNamesTooSimilar(NormalizeBusinessName(b.Name), normalizedBusinessName));
            if (BusinessNameExists)
            {
                ConflictingBusinessNameMessage = "The business name is too similar to an existing business.";
                return Page();
            }

            // Remove disabled days (OpenTime and CloseTime as TimeSpan.Zero indicate closed)
            Business.OpenHours = Business.OpenHours
                .Where(oh => oh.OpenTime != TimeSpan.Zero || oh.CloseTime != TimeSpan.Zero)
                .ToList();

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                Console.WriteLine("User is null. Cannot proceed with business creation.");
                return RedirectToPage("/Index");
            }

            Console.WriteLine("Associating business with user...");
            Business.UserBusinessRoles.Add(new UserBusinessRole
            {
                UserId = user.Id,
                Business = Business,
                Role = "Owner"
            });

            try
            {
                Console.WriteLine("Saving business to the database...");
                _context.Businesses.Add(Business);
                await _context.SaveChangesAsync();
                Console.WriteLine("Business created successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving business: {ex.Message}");
                return Page();
            }

            return RedirectToPage("/Businesses/Index");
        }

        public JsonResult OnGetValidateBusinessName(string name)
        {
            string normalizedName = NormalizeBusinessName(name);
            bool conflict = _context.Businesses
                .AsEnumerable() // Forces LINQ to execute in memory, allowing for custom methods
                .Any(b => AreNamesTooSimilar(NormalizeBusinessName(b.Name), normalizedName));

            return new JsonResult(new { conflict });
        }


        private string NormalizeBusinessName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }

            return Regex.Replace(name.ToUpperInvariant(), @"[^A-Z]", string.Empty); // Remove spaces, numbers, and special characters
        }

        private bool AreNamesTooSimilar(string existingName, string newName)
        {
            int distance = CalculateLevenshteinDistance(existingName, newName);
            double similarity = 1.0 - (double)distance / Math.Max(existingName.Length, newName.Length);

            // Stricter rule: consider names too similar if similarity is >= 80% 
            // AND the Levenshtein distance is <= 3
            return similarity >= 0.8 && distance <= 3;
        }

        private int CalculateLevenshteinDistance(string s, string t)
        {
            int[,] dp = new int[s.Length + 1, t.Length + 1];

            for (int i = 0; i <= s.Length; i++)
                dp[i, 0] = i;

            for (int j = 0; j <= t.Length; j++)
                dp[0, j] = j;

            for (int i = 1; i <= s.Length; i++)
            {
                for (int j = 1; j <= t.Length; j++)
                {
                    int cost = s[i - 1] == t[j - 1] ? 0 : 1;
                    dp[i, j] = Math.Min(Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1), dp[i - 1, j - 1] + cost);
                }
            }

            return dp[s.Length, t.Length];
        }

        //create the unique ID
        private string GenerateUniqueBusinessId()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[new Random().Next(s.Length)]).ToArray());
        }
    }
}
