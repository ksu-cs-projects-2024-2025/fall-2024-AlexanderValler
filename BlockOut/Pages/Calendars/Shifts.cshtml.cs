using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BlockOut.Data;
using BlockOut.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlockOut.Pages.Calendars
{
    public class ShiftsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ShiftsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Shift Shift { get; set; }

        [BindProperty(SupportsGet = true)]
        public string BusinessId { get; set; }

        public List<Shift> Shifts { get; set; } = new List<Shift>();
        public List<OpenHours> BusinessOpenHours { get; set; } = new List<OpenHours>();
        public string[] DaysOfWeek { get; } = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

        public async Task<IActionResult> OnGetAsync(string businessId)
        {
            if (string.IsNullOrWhiteSpace(businessId))
            {
                return NotFound("Business ID is required.");
            }

            BusinessId = businessId;

            var business = await _context.Businesses
                .Include(b => b.OpenHours)
                .FirstOrDefaultAsync(b => b.Id == businessId);

            if (business == null)
            {
                return NotFound("Business not found.");
            }

            // Convert OpenHours to ensure null safety and proper formatting
            BusinessOpenHours = business.OpenHours
                .Select(o => new OpenHours
                {
                    Day = o.Day,
                    OpenTime = o.OpenTime,
                    CloseTime = o.CloseTime
                })
                .ToList();

            Shifts = await _context.Shifts
                .Include(s => s.HourlyRequirements)
                .Where(s => s.BusinessId == businessId)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(List<ShiftHourRequirement> HourlyRequirements)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            Shift.BusinessId = BusinessId;
            Shift.HourlyRequirements = HourlyRequirements;

            _context.Shifts.Add(Shift);
            await _context.SaveChangesAsync();

            return RedirectToPage(new { businessId = BusinessId });
        }
    }
}
