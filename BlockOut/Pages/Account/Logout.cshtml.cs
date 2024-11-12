using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using BlockOut.Models; // Make sure to include this if ApplicationUser is in BlockOut.Models namespace

namespace BlockOut.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;

        public LogoutModel(SignInManager<ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _signInManager.SignOutAsync();
            return RedirectToPage("/Index"); // Redirect to the home page after logging out
        }
    }
}
