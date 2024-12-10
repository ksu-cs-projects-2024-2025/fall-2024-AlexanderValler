using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BlockOut.Data;
using BlockOut.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Text;

namespace BlockOut.Pages
{
    public class BusinessDeletionModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public BusinessDeletionModel(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public string EncodedId { get; set; }

        public string BusinessId { get; set; }

        public async Task<IActionResult> OnGetAsync(string encodedId)
        {
            if (string.IsNullOrWhiteSpace(encodedId))
            {
                return NotFound("Encoded business ID is required.");
            }

            try
            {
                BusinessId = DecodeBusinessId(encodedId);
            }
            catch
            {
                return BadRequest("Invalid encoded business ID format.");
            }

            var user = await _userManager.GetUserAsync(User);
            var business = await _context.Businesses
                .Include(b => b.UserBusinessRoles)
                .FirstOrDefaultAsync(b => b.Id == BusinessId);

            if (business == null)
            {
                return NotFound("Business not found.");
            }

            var isOwner = business.UserBusinessRoles.Any(ubr =>
                ubr.UserId == user.Id && ubr.Role == "Owner");

            if (!isOwner)
            {
                return Forbid();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string encodedId, string enteredBusinessId)
        {
            if (string.IsNullOrWhiteSpace(encodedId))
            {
                Console.WriteLine("Encoded ID is missing.");
                return BadRequest("Encoded ID is required.");
            }

            try
            {
                BusinessId = DecodeBusinessId(encodedId);
                Console.WriteLine($"Decoded BusinessId: {BusinessId}");
            }
            catch
            {
                Console.WriteLine("Failed to decode BusinessId.");
                return BadRequest("Invalid encoded business ID format.");
            }

            if (string.IsNullOrWhiteSpace(enteredBusinessId) || enteredBusinessId != BusinessId)
            {
                Console.WriteLine("Business ID mismatch.");
                ModelState.AddModelError("", "Business ID does not match.");
                return Page();
            }

            var business = await _context.Businesses
                .Include(b => b.UserBusinessRoles)
                .Include(b => b.Calendars)
                .Include(b => b.OpenHours)
                .Include(b => b.UserBusinessCalendars)
                .FirstOrDefaultAsync(b => b.Id == BusinessId);

            if (business == null)
            {
                Console.WriteLine("Business not found.");
                return NotFound("Business not found.");
            }

            try
            {
                Console.WriteLine("Deleting related entities...");
                var relatedRoles = _context.UserBusinessRoles.Where(ubr => ubr.BusinessId == business.Id);
                var relatedCalendars = _context.Calendars.Where(c => c.BusinessId == business.Id);
                var relatedOpenHours = _context.OpenHours.Where(oh => oh.BusinessId == business.Id);
                var relatedUserCalendars = _context.UserBusinessCalendars.Where(ubc => ubc.BusinessId == business.Id);

                _context.UserBusinessRoles.RemoveRange(relatedRoles);
                _context.Calendars.RemoveRange(relatedCalendars);
                _context.OpenHours.RemoveRange(relatedOpenHours);
                _context.UserBusinessCalendars.RemoveRange(relatedUserCalendars);
                _context.Businesses.Remove(business);

                await _context.SaveChangesAsync();
                Console.WriteLine("Business and related entities deleted successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during deletion: {ex.Message}");
                ModelState.AddModelError("", "Failed to delete the business. Please try again.");
                return Page();
            }

            Console.WriteLine("Redirecting to Dashboard...");
            TempData["SuccessMessage"] = "Business deleted successfully.";
            return RedirectToPage("/Dashboard");
        }



        private string DecodeBusinessId(string encodedId)
        {
            char[] charArray = encodedId.ToCharArray();
            Array.Reverse(charArray); // Reverse the encoded ID
            var base64 = new string(charArray)
                .Replace("-", "+")
                .Replace("_", "/");
            switch (base64.Length % 4) // Add padding if needed
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            var bytes = Convert.FromBase64String(base64);
            return Encoding.UTF8.GetString(bytes);
        }

    }
}
