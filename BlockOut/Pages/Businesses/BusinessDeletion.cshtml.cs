using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BlockOut.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using BlockOut.Models;
using System.Text;
using Microsoft.AspNetCore.Identity;
using System.Globalization;

namespace BlockOut.Pages.Businesses
{
    [Authorize]
    public class BusinessDeletionModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public BusinessDeletionModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public string BusinessId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string EncodedBusinessId { get; set; }

        public Business Business { get; set; }

        public async Task<IActionResult> OnGetAsync(string businessId)
        {

            if (string.IsNullOrWhiteSpace(businessId))
            {
                businessId = HttpContext.Session.GetString("CurrentBusinessId");
                if (string.IsNullOrWhiteSpace(businessId))
                {
                    return NotFound("Business ID is required.");
                }
            }
            else
            {
                try
                {
                    businessId = DecodeBusinessId(businessId);
                }
                catch (FormatException)
                {
                    return BadRequest("Invalid business ID format.");
                }

                HttpContext.Session.SetString("CurrentBusinessId", businessId);
            }

            Business = await _context.Businesses
                .Include(b => b.UserBusinessRoles)
                .ThenInclude(ubr => ubr.User)
                .Include(b => b.Calendars)
                .ThenInclude(c => c.UserBusinessCalendars)
                .ThenInclude(ubc => ubc.User)
                .Include(b => b.OpenHours) // Ensure OpenHours is included
                .FirstOrDefaultAsync(b => b.Id == businessId);

            if (Business == null)
            {
                return NotFound("Business not found.");
            }

            EncodedBusinessId = EncodeBusinessId(businessId);

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteBusinessAsync(string businessId)
        {
            if (string.IsNullOrWhiteSpace(businessId))
            {
                return BadRequest("Business ID is required.");
            }

            // Load the business and related entities
            var business = await _context.Businesses
                .Include(b => b.Calendars)
                .Include(b => b.OpenHours)
                .Include(b => b.UserBusinessRoles)
                .Include(b => b.UserBusinessCalendars)
                .FirstOrDefaultAsync(b => b.Id == businessId);

            if (business == null)
            {
                return NotFound("Business not found.");
            }

            // Remove associated data
            _context.Calendars.RemoveRange(business.Calendars);
            _context.OpenHours.RemoveRange(business.OpenHours);
            _context.UserBusinessRoles.RemoveRange(business.UserBusinessRoles);
            _context.UserBusinessCalendars.RemoveRange(business.UserBusinessCalendars);
            _context.Businesses.Remove(business);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Log the exception
                Console.WriteLine($"Error deleting business: {ex.Message}");
                return StatusCode(500, "Error occurred while deleting the business.");
            }

            return new JsonResult(new { success = true });
        }

        public async Task<IActionResult> OnPostAsync(string businessId)
        {
            if (string.IsNullOrWhiteSpace(businessId))
            {
                return BadRequest("Business ID is required.");
            }

            try
            {
                BusinessId = DecodeBusinessId(businessId);
            }
            catch (FormatException)
            {
                return BadRequest("Invalid business ID format.");
            }

            var business = await _context.Businesses
                .Include(b => b.Calendars)
                .Include(b => b.OpenHours)
                .Include(b => b.UserBusinessRoles)
                .Include(b => b.UserBusinessCalendars)
                .FirstOrDefaultAsync(b => b.Id == BusinessId);

            if (business == null)
            {
                return NotFound("Business not found.");
            }

            // Remove related entities
            _context.Calendars.RemoveRange(business.Calendars);
            _context.OpenHours.RemoveRange(business.OpenHours);
            _context.UserBusinessRoles.RemoveRange(business.UserBusinessRoles);
            _context.UserBusinessCalendars.RemoveRange(business.UserBusinessCalendars);

            // Remove the business
            _context.Businesses.Remove(business);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Businesses/Index");
        }

        private string EncodeBusinessId(string businessId)
        {
            var bytes = Encoding.UTF8.GetBytes(businessId);
            var base64 = Convert.ToBase64String(bytes);
            char[] charArray = base64.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        private string DecodeBusinessId(string encodedId)
        {
            char[] charArray = encodedId.ToCharArray();
            Array.Reverse(charArray);
            var bytes = Convert.FromBase64String(new string(charArray));
            return Encoding.UTF8.GetString(bytes);
        }


    }
}
