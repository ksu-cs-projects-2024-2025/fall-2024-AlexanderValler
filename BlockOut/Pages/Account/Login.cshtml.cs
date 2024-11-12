using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using BlockOut.Models;


namespace BlockOut.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public LoginModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public string ErrorMessage { get; set; }

        public void OnGet()
        {
            // Clear any previous error message
            ErrorMessage = string.Empty;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                // Normalize email to ensure correct matching
                var normalizedEmail = Email.ToUpper();
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail);

                if (user == null)
                {
                    ErrorMessage = "User not found.";
                    return Page();
                }

                // Check if the password is correct
                var passwordCheck = await _userManager.CheckPasswordAsync(user, Password);
                if (!passwordCheck)
                {
                    ErrorMessage = "Invalid password.";
                    return Page();
                }

                // Attempt to sign in
                var result = await _signInManager.PasswordSignInAsync(user.UserName, Password, isPersistent: false, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    return LocalRedirect(returnUrl ?? "/"); // Redirect to the return URL or home page
                }
                ErrorMessage = "Invalid login attempt.";
            }

            return Page();
        }
    }
}
