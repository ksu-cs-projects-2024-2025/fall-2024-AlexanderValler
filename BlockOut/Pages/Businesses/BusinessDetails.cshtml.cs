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
                return NotFound("Business ID is required.");
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

        public async Task<IActionResult> OnPostSaveOpenHoursAsync([FromBody] List<OpenHoursUpdateModel>? updatedOpenHours)
        {
            if (updatedOpenHours == null || !updatedOpenHours.Any())
            {
                Console.WriteLine("No open hours provided."); // Logging
                return BadRequest("No open hours provided.");
            }

            var businessId = HttpContext.Session.GetString("CurrentBusinessId");
            if (string.IsNullOrEmpty(businessId))
            {
                Console.WriteLine("Business ID is missing."); // Logging
                return BadRequest("Business ID is missing.");
            }

            // Retrieve the business entity
            var business = await _context.Businesses
                .Include(b => b.OpenHours)
                .FirstOrDefaultAsync(b => b.Id == businessId);

            if (business == null)
            {
                Console.WriteLine($"Business with ID {businessId} not found."); // Logging
                return NotFound("Business not found.");
            }

            foreach (var updatedHour in updatedOpenHours)
            {
                var existingHour = business.OpenHours.FirstOrDefault(oh => oh.Day == updatedHour.Day);

                try
                {
                    // Parse and convert OpenTime and CloseTime
                    var openTime = ParseTime(updatedHour.OpenTime);
                    var closeTime = ParseTime(updatedHour.CloseTime);

                    if (existingHour != null)
                    {
                        // Update existing record
                        existingHour.OpenTime = openTime;
                        existingHour.CloseTime = closeTime;
                        existingHour.IsClosed = updatedHour.IsClosed;
                    }
                    else
                    {
                        // Add new record if not existing
                        business.OpenHours.Add(new OpenHours
                        {
                            Day = updatedHour.Day,
                            OpenTime = openTime,
                            CloseTime = closeTime,
                            IsClosed = updatedHour.IsClosed,
                            BusinessId = business.Id
                        });
                    }
                }
                catch (FormatException ex)
                {
                    //Console.WriteLine($"Error parsing time for Day {updatedHour.Day}: {ex.Message}");
                    return BadRequest($"Invalid time format for Day {updatedHour.Day}. Please use HH:mm format.");
                }
            }

            await _context.SaveChangesAsync();

            //Console.WriteLine("Open hours updated successfully."); // Logging
            return new JsonResult(new { success = true });
        }

        // Helper method to parse time strings into TimeSpan
        private TimeSpan? ParseTime(string? timeString)
        {
            if (string.IsNullOrWhiteSpace(timeString))
            {
                return null; // Return null for empty strings (closed hours)
            }

            if (TimeSpan.TryParse(timeString, out var time))
            {
                return time;
            }

            throw new FormatException($"Invalid time format: {timeString}. Expected format is HH:mm.");
        }




        public class OpenHoursUpdateModel
        {
            public int Day { get; set; }
            public string? OpenTime { get; set; } // Nullable to allow for "Closed" days
            public string? CloseTime { get; set; } // Nullable to allow for "Closed" days
            public bool IsClosed { get; set; } // Indicates if the business is closed
        }


        public async Task<IActionResult> OnPostEditBusinessNameAsync([FromBody] BusinessNameUpdateModel model)
        {
            if (string.IsNullOrWhiteSpace(model?.Name) || string.IsNullOrWhiteSpace(model.Id))
            {
                return BadRequest(new { success = false, message = "Business name or ID cannot be empty." });
            }

            var business = await _context.Businesses.FirstOrDefaultAsync(b => b.Id == model.Id);
            if (business == null)
            {
                return NotFound(new { success = false, message = "Business not found." });
            }

            var allBusinesses = await _context.Businesses
                .Where(b => b.Id != model.Id)
                .ToListAsync();

            var normalizedNewName = NormalizeBusinessName(model.Name);
            if (allBusinesses.Any(b => NormalizeBusinessName(b.Name) == normalizedNewName))
            {
                return BadRequest(new { success = false, message = "The business name is too similar to an existing one." });
            }

            business.Name = model.Name;
            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true });
        }


        public class BusinessNameUpdateModel
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }

        public class RemoveCalendarRequest
        {
            public string CalendarId { get; set; }
        }

        public async Task<IActionResult> OnPostRemoveCalendarAsync([FromBody] RemoveCalendarRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.CalendarId))
            {
                return BadRequest("Invalid calendar ID.");
            }

            var calendar = await _context.Calendars
                .Include(c => c.Business)
                .FirstOrDefaultAsync(c => c.Id == request.CalendarId);

            if (calendar == null || calendar.BusinessId != Business.Id)
            {
                return NotFound("Calendar not found.");
            }

            if (!IsOwnerOrManager)
            {
                return Forbid();
            }

            _context.Calendars.Remove(calendar);
            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true });
        }



        private string EncodeBusinessId(string businessId)
        {
            var bytes = Encoding.UTF8.GetBytes(businessId);
            var base64 = Convert.ToBase64String(bytes)
                .Replace("=", "") // Remove padding
                .Replace("+", "-") // Replace URL-unsafe characters
                .Replace("/", "_"); // Replace URL-unsafe characters

            char[] charArray = base64.ToCharArray();
            Array.Reverse(charArray); // Reverse for added obfuscation
            return new string(charArray);
        }


        private string DecodeBusinessId(string encodedId)
        {
            char[] charArray = encodedId.ToCharArray();
            Array.Reverse(charArray); // Reverse to original Base64

            var base64 = new string(charArray)
                .Replace("-", "+") // Restore Base64 characters
                .Replace("_", "/"); // Restore Base64 characters

            // Add padding if missing
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }

            var bytes = Convert.FromBase64String(base64);
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
