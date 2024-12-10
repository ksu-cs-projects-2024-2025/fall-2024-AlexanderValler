using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BlockOut.Data;
using BlockOut.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlockOut.Pages.Calendars
{
    public class CreateCalendarModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public List<Shift> AvailableShifts { get; set; } = new List<Shift>();

        public CreateCalendarModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string CalendarName { get; set; }

        [BindProperty]
        public string BusinessId { get; set; }

        public List<UserBusinessRole> BusinessMembers { get; set; } = new List<UserBusinessRole>();
        public string[] DaysOfWeek { get; } = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

        public async Task<IActionResult> OnGetAsync(string businessId)
        {
            if (string.IsNullOrWhiteSpace(businessId))
            {
                return NotFound("Business ID is required.");
            }

            BusinessId = businessId;

            var business = await _context.Businesses
                .Include(b => b.UserBusinessRoles)
                .ThenInclude(ubr => ubr.User)
                .FirstOrDefaultAsync(b => b.Id == businessId);

            if (business == null)
            {
                return NotFound("Business not found.");
            }

            BusinessMembers = business.UserBusinessRoles.ToList();

            AvailableShifts = await _context.Shifts
                .Where(s => s.BusinessId == businessId)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync([FromForm] List<string> selectedShifts)
        {
            if (string.IsNullOrWhiteSpace(CalendarName) || string.IsNullOrWhiteSpace(BusinessId))
            {
                ModelState.AddModelError(string.Empty, "Calendar name and business ID are required.");
                return Page();
            }

            var business = await _context.Businesses.FirstOrDefaultAsync(b => b.Id == BusinessId);

            if (business == null)
            {
                ModelState.AddModelError(string.Empty, "Business not found.");
                return Page();
            }

            var newCalendar = new Calendar
            {
                Id = Guid.NewGuid().ToString(),
                Name = CalendarName,
                BusinessId = BusinessId,
                Type = "Shift"
            };

            if (selectedShifts != null && selectedShifts.Any())
            {
                foreach (var shiftId in selectedShifts)
                {
                    var shift = await _context.Shifts.FirstOrDefaultAsync(s => s.Id == int.Parse(shiftId));
                    if (shift != null)
                    {
                        shift.CalendarId = newCalendar.Id;
                    }
                }
            }

            _context.Calendars.Add(newCalendar);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Calendars/View", new { calendarId = newCalendar.Id });
        }
    }
}
