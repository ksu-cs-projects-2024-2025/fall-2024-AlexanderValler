using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BlockOut.Data;
using BlockOut.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace BlockOut.Pages.Dashboard
{
    [Authorize] // Restrict access to logged-in users
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public class JoinBusinessModel
        {
            public string BusinessId { get; set; }
        }

        public List<UserBusinessDisplay> UserBusinesses { get; set; }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            var userBusinessRoles = await _context.UserBusinessRoles
                .Include(ubr => ubr.Business)
                .Where(ubr => ubr.UserId == user.Id)
                .ToListAsync();

            UserBusinesses = userBusinessRoles.Select(ubr => new UserBusinessDisplay
            {
                Name = ubr.Business.Name,
                Role = ubr.Role,
                EncodedBusinessId = EncodeBusinessId(ubr.Business.Id)
            }).ToList();
        }

        public async Task<IActionResult> OnPostJoinBusinessAsync([FromBody] JoinBusinessModel model)
        {
            if (string.IsNullOrWhiteSpace(model.BusinessId))
            {
                return BadRequest(new { success = false, message = "Business ID cannot be empty." });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            // Check if the business exists
            var business = await _context.Businesses.FirstOrDefaultAsync(b => b.Id == model.BusinessId);
            if (business == null)
            {
                return NotFound(new { success = false, message = "Business not found." });
            }

            // Check if the user is already part of the business
            var existingRole = await _context.UserBusinessRoles
                .FirstOrDefaultAsync(ubr => ubr.UserId == user.Id && ubr.BusinessId == model.BusinessId);

            if (existingRole != null)
            {
                return BadRequest(new { success = false, message = "You are already in this business." });
            }

            // Add the user to the business as an employee
            var newUserRole = new UserBusinessRole
            {
                UserId = user.Id,
                BusinessId = model.BusinessId,
                Role = "Employee" // Default role for joining
            };

            _context.UserBusinessRoles.Add(newUserRole);
            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true });
        }

        private string EncodeBusinessId(string businessId)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(businessId);
            var base64 = Convert.ToBase64String(bytes);
            char[] charArray = base64.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
    }

    public class UserBusinessDisplay
    {
        public string Name { get; set; }
        public string Role { get; set; }
        public string EncodedBusinessId { get; set; }
    }
}
