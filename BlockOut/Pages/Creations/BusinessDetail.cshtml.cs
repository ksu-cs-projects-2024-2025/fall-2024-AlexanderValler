using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BlockOut.Models;
using BlockOut.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace BlockOut.Pages.Creations
{
    [Authorize]
    public class BusinessDetailModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BusinessDetailModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public Business Business { get; set; }
        public string Role { get; set; }

        public async Task<IActionResult> OnGetAsync(int businessId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized();
            }

            // Fetch the business
            Business = await _context.Businesses.FindAsync(businessId);
            if (Business == null)
            {
                return NotFound();
            }

            // Check if the user has a role in this business
            var userBusinessRole = await _context.UserBusinessRoles
                .FirstOrDefaultAsync(ubr => ubr.BusinessId == businessId && ubr.UserId == user.Id);

            if (userBusinessRole == null)
            {
                return Forbid();
            }

            // Set the user's role for use in the page
            Role = userBusinessRole.Role;

            return Page();
        }
    }
}
