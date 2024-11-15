using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BlockOut.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;


namespace BlockOut.Pages.Account
{
    [Authorize] // Ensures only authenticated users can access this page
    public class ProfileModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ProfileModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public string UserName { get; set; }
        public string Email { get; set; }
        public int ProfilePictureId { get; set; }

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

        public async Task<IActionResult> OnPostUpdateNameAsync([FromBody] NameUpdateModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Name))
            {
                return BadRequest("Name cannot be empty.");
            }

            if (model.Name.Length < 2)
            {
                return BadRequest("Name must be at least 2 characters long.");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            user.UserName = model.Name;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return StatusCode(500, "Failed to update name.");
            }

            // Update authentication claims
            await _signInManager.RefreshSignInAsync(user);

            return new JsonResult(new { success = true });
        }

        public class NameUpdateModel
        {
            public string Name { get; set; }
        }


        public async Task<IActionResult> OnPostUpdateProfilePictureAsync([FromBody] ProfilePictureUpdateModel model)
        {
            if (model == null || model.ProfilePictureId < 1 || model.ProfilePictureId > 6)
            {
                return BadRequest(new { success = false, message = "Invalid profile picture ID." });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound(new { success = false, message = "User not found." });
            }

            // Update the profile picture ID
            user.ProfilePictureId = model.ProfilePictureId;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return StatusCode(500, new { success = false, message = "Failed to update profile picture." });
            }

            return new JsonResult(new { success = true });
        }

        // Model for updating profile picture
        public class ProfilePictureUpdateModel
        {
            public int ProfilePictureId { get; set; }
        }
    }
}
