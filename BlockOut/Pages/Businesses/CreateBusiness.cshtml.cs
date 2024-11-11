using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BlockOut.Models;
using BlockOut.Data;
using System.Threading.Tasks;

namespace BlockOut.Pages.Businesses
{
    [Authorize]
    public class CreateBusinessModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public CreateBusinessModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public Business Business { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            Business.UserId = user.Id;

            // Save the new business to the database
            _context.Businesses.Add(Business);
            await _context.SaveChangesAsync();

            // Redirect to the dashboard
            return RedirectToPage("/Dashboard");
        }
    }
}