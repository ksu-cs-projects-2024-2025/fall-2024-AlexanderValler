using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BlockOut.Data;
using BlockOut.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlockOut.Pages.Businesses
{
    [Authorize]
    public class ManageEmployeesModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ManageEmployeesModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public string EncodedId { get; set; }

        public string BusinessId { get; set; }
        public List<UserBusinessRole> OwnerRoles { get; set; } = new List<UserBusinessRole>();
        public List<UserBusinessRole> ManagerRoles { get; set; } = new List<UserBusinessRole>();
        public List<UserBusinessRole> EmployeeRoles { get; set; } = new List<UserBusinessRole>();
        public bool IsOwner { get; set; }
        public bool IsManagerOrOwner { get; set; }
        public string InviteCode { get; set; }

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

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            var business = await _context.Businesses
                .Include(b => b.UserBusinessRoles)
                .ThenInclude(ubr => ubr.User)
                .FirstOrDefaultAsync(b => b.Id == BusinessId);

            if (business == null)
            {
                return NotFound("Business not found.");
            }

            var userRole = business.UserBusinessRoles.FirstOrDefault(ubr => ubr.UserId == user.Id);

            if (userRole == null)
            {
                return Forbid();
            }

            IsOwner = userRole.Role == "Owner";
            IsManagerOrOwner = IsOwner || userRole.Role == "Manager";

            OwnerRoles = await _context.UserBusinessRoles
                .Where(ubr => ubr.BusinessId == BusinessId && ubr.Role == "Owner")
                .Include(ubr => ubr.User)
                .ToListAsync();

            ManagerRoles = await _context.UserBusinessRoles
                .Where(ubr => ubr.BusinessId == BusinessId && ubr.Role == "Manager")
                .Include(ubr => ubr.User)
                .ToListAsync();

            EmployeeRoles = await _context.UserBusinessRoles
                .Where(ubr => ubr.BusinessId == BusinessId && ubr.Role == "Employee")
                .Include(ubr => ubr.User)
                .ToListAsync();

            InviteCode = BusinessId; // Return the raw business ID
            return Page();
        }

        public async Task<IActionResult> OnPostChangeRoleAsync([FromBody] RoleChangeModel model)
        {
            if (string.IsNullOrWhiteSpace(model.UserId) || string.IsNullOrWhiteSpace(model.Role) || string.IsNullOrWhiteSpace(model.BusinessId))
            {
                return BadRequest("Invalid input.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            var business = await _context.Businesses.Include(b => b.UserBusinessRoles).FirstOrDefaultAsync(b => b.Id == model.BusinessId);

            if (business == null || user == null)
            {
                return NotFound("Business or user not found.");
            }

            var userRole = business.UserBusinessRoles.FirstOrDefault(ubr => ubr.UserId == user.Id);

            if (userRole == null || (!IsOwner && userRole.Role == "Manager" && model.Role == "Owner"))
            {
                return Forbid();
            }

            var targetRole = business.UserBusinessRoles.FirstOrDefault(ubr => ubr.UserId == model.UserId);

            if (targetRole == null)
            {
                return NotFound("Target user role not found.");
            }

            if (targetRole.Role == "Owner" && model.Role != "Owner")
            {
                var ownersCount = business.UserBusinessRoles.Count(ubr => ubr.Role == "Owner");
                if (ownersCount <= 1)
                {
                    return BadRequest("There must always be at least one owner.");
                }
            }

            if (model.Role == "remove")
            {
                _context.UserBusinessRoles.Remove(targetRole);
            }
            else
            {
                targetRole.Role = model.Role;
                _context.UserBusinessRoles.Update(targetRole);
            }

            await _context.SaveChangesAsync();
            await ReloadRolesAsync(business.Id);

            return new JsonResult(new { success = true });
        }

        private async Task ReloadRolesAsync(string businessId)
        {
            OwnerRoles = await _context.UserBusinessRoles
                .Where(ubr => ubr.BusinessId == businessId && ubr.Role == "Owner")
                .Include(ubr => ubr.User)
                .ToListAsync();

            ManagerRoles = await _context.UserBusinessRoles
                .Where(ubr => ubr.BusinessId == businessId && ubr.Role == "Manager")
                .Include(ubr => ubr.User)
                .ToListAsync();

            EmployeeRoles = await _context.UserBusinessRoles
                .Where(ubr => ubr.BusinessId == businessId && ubr.Role == "Employee")
                .Include(ubr => ubr.User)
                .ToListAsync();
        }

        private string DecodeBusinessId(string encodedId)
        {
            char[] charArray = encodedId.ToCharArray();
            Array.Reverse(charArray);

            var base64 = new string(charArray)
                .Replace("-", "+")
                .Replace("_", "/");

            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }

            var bytes = Convert.FromBase64String(base64);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }

        public class RoleChangeModel
        {
            public string UserId { get; set; }
            public string Role { get; set; }
            public string BusinessId { get; set; }
        }
    }
}
