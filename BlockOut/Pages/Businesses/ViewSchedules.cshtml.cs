using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BlockOut.Data;
using BlockOut.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlockOut.Pages.Businesses
{
    [Authorize]
    public class ViewScheduleModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ViewScheduleModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public string BusinessId { get; set; }

        public Business Business { get; set; }
        public List<Calendar> Calendars { get; set; } = new List<Calendar>();

        public async Task<IActionResult> OnGetAsync(string businessId)
        {
            if (string.IsNullOrWhiteSpace(businessId))
            {
                return NotFound("Business ID is required.");
            }

            Business = await _context.Businesses
                .Include(b => b.Calendars)
                .FirstOrDefaultAsync(b => b.Id == businessId);

            if (Business == null)
            {
                return NotFound("Business not found.");
            }

            Calendars = Business.Calendars.ToList();
            return Page();
        }
    }
}
