using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BlockOut.Models;
using System.Threading.Tasks;

namespace BlockOut.Pages.Account
{
    public class ProfileModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public string UserName { get; set; }
        public string Email { get; set; }
        public int ProfilePictureId { get; set; } // New property for the profile picture ID

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                UserName = user.UserName;
                Email = user.Email;
                ProfilePictureId = user.ProfilePictureId ?? 1; // Default to integer 1


            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Ensures CSRF protection for the POST request
        public async Task<IActionResult> OnPostUpdateProfilePictureAsync([FromBody] ProfilePictureUpdateModel model)
        {
            if (model == null || model.ProfilePictureId < 1 || model.ProfilePictureId > 6)
            {
                return BadRequest("Invalid profile picture ID.");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Update the profile picture ID
            user.ProfilePictureId = model.ProfilePictureId;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                // Return error details if the update failed
                return StatusCode(500, "Failed to update profile picture.");
            }

            return new JsonResult(new { success = true });
        }
    }

    // Model to receive profile picture update request
    public class ProfilePictureUpdateModel
    {
        public int ProfilePictureId { get; set; }
    }
}
