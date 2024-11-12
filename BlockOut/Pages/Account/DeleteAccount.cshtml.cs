using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using BlockOut.Models;

namespace BlockOut.Pages.Account
{
    public class DeleteAccountModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public DeleteAccountModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Index"); // User not found, redirect to home
            }

            // Remove the user
            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                await _signInManager.SignOutAsync(); // Log out the user after deletion
                return RedirectToPage("/Index"); // Redirect to homepage
            }

            // Handle errors (display to user, log, etc.)
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }
    }
}
