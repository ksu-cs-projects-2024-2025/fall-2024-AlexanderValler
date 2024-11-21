using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BlockOut.Models;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace BlockOut.Pages.Account
{
    public class ResetModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ResetModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public required string Email { get; set; }
        }

        public void OnGet()
        {
            // Display the reset password form
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Server-side check for email existence
            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Email does not exist.");
                return Page();
            }

            // Future implementation for sending reset email
            // For now, return to the same page with a success message
            TempData["Success"] = "If this email exists, a reset link has been sent.";
            return RedirectToPage("/Account/Reset");
        }

        public async Task<IActionResult> OnPostCheckEmailExistsAsync([FromBody] CheckEmailModel model)
        {
            if (string.IsNullOrEmpty(model?.Email))
            {
                return BadRequest(new { exists = false });
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            return new JsonResult(new { exists = user != null });
        }

        public class CheckEmailModel
        {
            public string? Email { get; set; }
        }
    }
}
