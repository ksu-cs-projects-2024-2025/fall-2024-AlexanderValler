using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using BlockOut.Models;
using BlockOut.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace BlockOut.Pages.Businesses
{
    [Authorize]
    public class BusinessDetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public BusinessDetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Business Business { get; set; }

        public List<UserBusinessRole> UserBusinessRoles { get; set; } = new List<UserBusinessRole>();

        [BindProperty(SupportsGet = true)]
        public string BusinessId { get; set; }

        public bool IsOwnerOrManager { get; private set; }

        public async Task<IActionResult> OnGetAsync(string businessId)
        {
            if (string.IsNullOrWhiteSpace(businessId))
            {
                return NotFound("Business ID is required.");
            }

            // Load the business and related roles
            Business = await _context.Businesses
                .Include(b => b.UserBusinessRoles)
                .ThenInclude(ubr => ubr.User)
                .FirstOrDefaultAsync(b => b.Id == businessId);

            if (Business == null)
            {
                return NotFound("Business not found.");
            }

            UserBusinessRoles = Business.UserBusinessRoles;

            // Retrieve the current user's ID from the authentication claims
            var currentUserId = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            if (string.IsNullOrEmpty(currentUserId))
            {
                // If we cannot retrieve the current user's ID, default IsOwnerOrManager to false
                IsOwnerOrManager = false;
                return Page();
            }

            // Check if the user has the Owner or Manager role
            IsOwnerOrManager = UserBusinessRoles.Any(ubr =>
                ubr.UserId == currentUserId && (ubr.Role == "Owner" || ubr.Role == "Manager"));

            return Page();
        }

        public async Task<IActionResult> OnPostEditBusinessNameAsync(string id)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var business = await _context.Businesses.FindAsync(id);
            if (business == null)
            {
                return NotFound();
            }

            business.Name = Business.Name;
            await _context.SaveChangesAsync();

            return RedirectToPage(new { id });
        }

        public class BusinessUpdateModel
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }
    }
}
