using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BlockOut.Models;
using BlockOut.Data;
using System.Threading.Tasks;

namespace BlockOut.Pages.Creations
{
    [Authorize]
    public class BusinessDetailModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public BusinessDetailModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public Business Business { get; set; }
        public string Role { get; set; }

        public async Task<IActionResult> OnGetAsync(int businessId)
        {
            var user = await _userManager.GetUserAsync(User);
            Business = await _context.Businesses.FindAsync(businessId);

            if (Business == null || Business.UserId != user.Id)
            {
                return NotFound();
            }

            Role = Business.Role; // Set the role for use in the page

            return Page();
        }
    }
}
