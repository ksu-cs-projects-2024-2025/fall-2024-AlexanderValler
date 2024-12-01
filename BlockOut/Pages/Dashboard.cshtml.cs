using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BlockOut.Data;
using BlockOut.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlockOut.Pages.Dashboard
{
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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
