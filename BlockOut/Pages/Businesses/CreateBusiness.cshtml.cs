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
            Business.OpenHours = Enumerable.Range(1, 7).Select(day => new OpenHours
            {
                Day = day,
                OpenTime = new TimeSpan(9, 0, 0),
                CloseTime = new TimeSpan(17, 0, 0),
                IsClosed = false
            }).ToList();

            ConflictingBusinessNameMessage = string.Empty;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Console.WriteLine("OnPostAsync triggered");

            if (string.IsNullOrEmpty(Business.Id))
            {
                Business.Id = GenerateUniqueBusinessId();
                Console.WriteLine($"Generated Business Id: {Business.Id}");
            }

            ConflictingBusinessNameMessage ??= string.Empty;

            ModelState.Remove("Business.Id");
            ModelState.Remove("ConflictingBusinessNameMessage");

            if (!ModelState.IsValid)
            {
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    foreach (var error in state.Errors)
                    {
                        Console.WriteLine($"Key: {key}, Error: {error.ErrorMessage}");
                    }
                }
                return Page();
            }

            foreach (var openHour in Business.OpenHours)
            {
                openHour.BusinessId = Business.Id;
            }

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
                _context.Businesses.Add(Business);
                await _context.SaveChangesAsync();
                Console.WriteLine("Business created successfully.");
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
