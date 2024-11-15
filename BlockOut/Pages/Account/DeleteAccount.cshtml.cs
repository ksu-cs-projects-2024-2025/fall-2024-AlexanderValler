using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using BlockOut.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BlockOut.Data;

namespace BlockOut.Pages.Account
{
    [Authorize] // Ensures only authenticated users can access this page
    public class DeleteAccountModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context; // Injecting the database context

        public DeleteAccountModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Index"); // User not found, redirect to home
            }

            // Fetch businesses where the user is the owner
            var ownedBusinesses = await _context.UserBusinessRoles
                .Include(ubr => ubr.Business)
                .Where(ubr => ubr.UserId == user.Id && ubr.Role == "Owner")
                .Select(ubr => ubr.Business)
                .ToListAsync();

            foreach (var business in ownedBusinesses)
            {
                // Find managers for the business
                var managers = await _context.UserBusinessRoles
                    .Where(ubr => ubr.BusinessId == business.Id && ubr.Role == "Manager")
                    .ToListAsync();

                if (managers.Any())
                {
                    // Transfer ownership to the first manager found
                    var newOwner = managers.First();
                    var ownerRole = await _context.UserBusinessRoles
                        .FirstOrDefaultAsync(ubr => ubr.BusinessId == business.Id && ubr.UserId == user.Id && ubr.Role == "Owner");

                    if (ownerRole != null)
                    {
                        // Update the role to "Manager" for the current user
                        ownerRole.Role = "Manager";
                        _context.UserBusinessRoles.Update(ownerRole);
                    }

                    // Update the manager's role to "Owner"
                    newOwner.Role = "Owner";
                    _context.UserBusinessRoles.Update(newOwner);
                }
                else
                {
                    // No managers exist, delete the business and related roles
                    var relatedRoles = _context.UserBusinessRoles
                        .Where(ubr => ubr.BusinessId == business.Id);

                    _context.UserBusinessRoles.RemoveRange(relatedRoles);
                    _context.Businesses.Remove(business);
                }
            }

            // Save changes to the database
            await _context.SaveChangesAsync();

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

            return Page(); // Stay on the same page if deletion fails
        }
    }
}
