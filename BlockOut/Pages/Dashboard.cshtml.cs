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

        public List<UserBusinessRole> UserBusinesses { get; set; }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            // Fetch all businesses the user is part of, including their role
            UserBusinesses = await _context.UserBusinessRoles
                .Include(ubr => ubr.Business)
                .Where(ubr => ubr.UserId == user.Id)
                .ToListAsync();
        }
    }
}
