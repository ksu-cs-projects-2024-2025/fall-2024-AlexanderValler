using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BlockOut.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Linq;
using BlockOut.Data;

namespace BlockOut.Pages.Account
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public ProfileModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        public string UserName { get; set; }
        public string Email { get; set; }
        public int ProfilePictureId { get; set; }
        public List<BusinessRoleModel> Businesses { get; set; } = new List<BusinessRoleModel>();

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                UserName = user.UserName;
                Email = user.Email;
                ProfilePictureId = user.ProfilePictureId ?? 1; // Default to integer 1

                // Fetch businesses and roles
                Businesses = await _context.UserBusinessRoles
                    .Where(ubr => ubr.UserId == user.Id)
                    .Include(ubr => ubr.Business)
                    .Select(ubr => new BusinessRoleModel
                    {
                        Name = ubr.Business.Name,
                        Role = ubr.Role
                    }).ToListAsync();
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

        public async Task<IActionResult> OnPostUpdateEmailAsync([FromBody] EmailUpdateModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Email))
            {
                return BadRequest("Email cannot be empty.");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Check if email is already in use
            if (await _userManager.FindByEmailAsync(model.Email) != null)
            {
                return BadRequest("Email is already in use.");
            }

            user.Email = model.Email;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return StatusCode(500, "Failed to update email.");
            }

            await _signInManager.RefreshSignInAsync(user);

            return new JsonResult(new { success = true });
        }

        public async Task<IActionResult> OnPostLeaveBusinessAsync([FromBody] BusinessLeaveModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.BusinessName))
            {
                return BadRequest("Invalid business name.");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var businessRole = await _context.UserBusinessRoles
                .Include(ubr => ubr.Business)
                .FirstOrDefaultAsync(ubr => ubr.UserId == user.Id && ubr.Business.Name == model.BusinessName);

            if (businessRole == null)
            {
                return NotFound("User is not part of this business.");
            }

            _context.UserBusinessRoles.Remove(businessRole);
            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true });
        }

        // Model for updating name
        public class NameUpdateModel
        {
            public string? Name { get; set; }
        }

        // Model for updating profile picture
        public class ProfilePictureUpdateModel
        {
            public int ProfilePictureId { get; set; }
        }

        // Model for updating email
        public class EmailUpdateModel
        {
            public string Email { get; set; }
        }

        // Model for leaving a business
        public class BusinessLeaveModel
        {
            public string BusinessName { get; set; }
        }

        // Model for businesses and roles
        public class BusinessRoleModel
        {
            public string Name { get; set; }
            public string Role { get; set; }
        }
    }
}
