using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BlockOut.Data;
using BlockOut.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace BlockOut.Pages.Businesses
{
    [Authorize]
    public class BusinessDetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BusinessDetailsModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public Business Business { get; set; }

        [BindProperty(SupportsGet = true)]
        public string BusinessId { get; set; }

        public string EncodedBusinessId { get; set; }

        public List<UserBusinessRole> UserBusinessRoles { get; set; } = new List<UserBusinessRole>();
        public List<Calendar> Calendars { get; set; } = new List<Calendar>();

        public bool IsOwner { get; set; } = false;
        public bool IsOwnerOrManager { get; set; } = false;

        // Days of the week for display
        public string[] DaysOfWeek { get; } = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

        public async Task<IActionResult> OnGetAsync(string businessId)
        {
            if (string.IsNullOrWhiteSpace(businessId))
            {
                businessId = HttpContext.Session.GetString("CurrentBusinessId");
                if (string.IsNullOrWhiteSpace(businessId))
                {
                    return NotFound("Business ID is required.");
                }
            }
            else
            {
                try
                {
                    businessId = DecodeBusinessId(businessId);
                }
                catch (FormatException)
                {
                    return BadRequest("Invalid business ID format.");
                }

                HttpContext.Session.SetString("CurrentBusinessId", businessId);
            }

            // Load the business with all its relationships
            Business = await _context.Businesses
                .Include(b => b.UserBusinessRoles)
                .ThenInclude(ubr => ubr.User)
                .Include(b => b.Calendars)
                .ThenInclude(c => c.UserBusinessCalendars)
                .ThenInclude(ubc => ubc.User)
                .Include(b => b.OpenHours) // Ensure OpenHours is included
                .FirstOrDefaultAsync(b => b.Id == businessId);

            if (Business == null)
            {
                return NotFound("Business not found.");
            }

            UserBusinessRoles = Business.UserBusinessRoles;

            var user = await _userManager.GetUserAsync(User);
            IsOwnerOrManager = UserBusinessRoles.Any(ubr =>
                ubr.UserId == user.Id && (ubr.Role == "Owner" || ubr.Role == "Manager"));

            IsOwner = UserBusinessRoles.Any(ubr => ubr.UserId == user.Id && ubr.Role == "Owner");

            Calendars = IsOwnerOrManager
                ? Business.Calendars
                : Business.Calendars.Where(c => c.UserBusinessCalendars.Any(ubc => ubc.UserId == user.Id)).ToList();

            // Handle missing OpenHours entries
            if (Business.OpenHours == null || Business.OpenHours.Count != 7)
            {
                var existingDays = Business.OpenHours?.Select(oh => oh.Day).ToHashSet() ?? new HashSet<int>();
                var missingDays = Enumerable.Range(1, 7).Except(existingDays);

                foreach (var day in missingDays)
                {
                    Business.OpenHours.Add(new OpenHours
                    {
                        Day = day,
                        IsClosed = true,
                        OpenTime = null,
                        CloseTime = null,
                        BusinessId = Business.Id
                    });
                }

                await _context.SaveChangesAsync(); // Save any newly added OpenHours
            }

            EncodedBusinessId = EncodeBusinessId(businessId);

            return Page();
        }

        private string EncodeBusinessId(string businessId)
        {
            var bytes = Encoding.UTF8.GetBytes(businessId);
            var base64 = Convert.ToBase64String(bytes);
            char[] charArray = base64.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        private string DecodeBusinessId(string encodedId)
        {
            char[] charArray = encodedId.ToCharArray();
            Array.Reverse(charArray);
            var bytes = Convert.FromBase64String(new string(charArray));
            return Encoding.UTF8.GetString(bytes);
        }

        private string NormalizeBusinessName(string name)
        {
            return Regex.Replace(name?.ToUpperInvariant() ?? string.Empty, @"[^A-Z]", string.Empty);
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
                    dp[i, j] = Math.Min(Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1), dp[i - 1, j - 1] + cost);
                }
            }
            return dp[s.Length, t.Length];
        }
    }
}
