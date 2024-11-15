using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BlockOut.Models;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace BlockOut.Pages.Account
{
    public class CreateAccountModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public CreateAccountModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Name")]
            [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters.")]
            public string Name { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [StringLength(100, ErrorMessage = "Password must be at least 8 characters long.", MinimumLength = 8)]
            [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
                ErrorMessage = "Password must have at least one uppercase letter, one lowercase letter, one digit, and one special character.")]
            public string Password { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Confirm Password")]
            [Compare("Password", ErrorMessage = "Passwords do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public void OnGet()
        {
            // Show the form
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Server-side email check
            var existingUser = await _userManager.FindByEmailAsync(Input.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError(string.Empty, "An account with this email already exists.");
                return Page();
            }

            // Create the new user
            var newUser = new ApplicationUser
            {
                UserName = Input.Name,
                Email = Input.Email,
            };

            var result = await _userManager.CreateAsync(newUser, Input.Password);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(newUser, isPersistent: false);
                return RedirectToPage("/Dashboard");
            }

            // Handle errors
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }


        public async Task<IActionResult> OnPostCheckEmailInUseAsync([FromBody] CheckEmailModel model)
        {
            if (string.IsNullOrEmpty(model?.Email))
            {
                return BadRequest(new { isInUse = false });
            }

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            return new JsonResult(new { isInUse = existingUser != null });
        }

        // Model for email check request
        public class CheckEmailModel
        {
            public string Email { get; set; }
        }


    }
}