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
using Microsoft.EntityFrameworkCore;

namespace BlockOut.Pages.Businesses
{
    [Authorize] // Ensures only authenticated users can access this page
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

        public List<UserBusinessRole> UserBusinessRoles { get; set; } = new List<UserBusinessRole>();

        public bool IsOwnerOrManager { get; set; } = false;

        public async Task<IActionResult> OnGetAsync(string businessId)
        {
            if (string.IsNullOrWhiteSpace(businessId))
            {
                return NotFound("Business ID is required.");
            }

            Business = await _context.Businesses
                .Include(b => b.UserBusinessRoles)
                .ThenInclude(ubr => ubr.User)
                .FirstOrDefaultAsync(b => b.Id == businessId);

            if (Business == null)
            {
                return NotFound("Business not found.");
            }

            UserBusinessRoles = Business.UserBusinessRoles;

            // Check if the logged-in user is an Owner or Manager of this business
            var user = await _userManager.GetUserAsync(User);
            IsOwnerOrManager = UserBusinessRoles.Any(ubr =>
                ubr.UserId == user.Id && (ubr.Role == "Owner" || ubr.Role == "Manager"));

            return Page();
        }

        public async Task<IActionResult> OnPostEditBusinessNameAsync([FromBody] BusinessUpdateModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Name))
            {
                return BadRequest(new { success = false, message = "Invalid business name." });
            }

            // Validate for duplicates or too-similar names
            string normalizedBusinessName = NormalizeBusinessName(model.Name);

            bool isNameConflict = _context.Businesses
                .AsEnumerable() // Forces LINQ to execute in memory, allowing for custom methods
                .Any(b => b.Id != model.Id && AreNamesTooSimilar(NormalizeBusinessName(b.Name), normalizedBusinessName));

            if (isNameConflict)
            {
                return BadRequest(new { success = false, message = "The business name is too similar to an existing business." });
            }

            var business = await _context.Businesses.FindAsync(model.Id);
            if (business == null)
            {
                return NotFound(new { success = false, message = "Business not found." });
            }

            business.Name = model.Name;

            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true });
        }

        public JsonResult OnGetValidateBusinessName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return new JsonResult(new { conflict = false });
            }

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

        public class BusinessUpdateModel
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }
    }
}
