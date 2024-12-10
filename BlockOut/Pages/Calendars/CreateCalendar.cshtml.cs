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

        public CreateCalendarModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string CalendarName { get; set; }

        [BindProperty]
        public string CalendarDescription { get; set; }

        [BindProperty]
        public string BusinessId { get; set; }

        public List<UserBusinessRole> BusinessMembers { get; set; } = new List<UserBusinessRole>();
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
                .Include(b => b.UserBusinessRoles)
                .ThenInclude(ubr => ubr.User)
                .Include(b => b.OpenHours)
                .FirstOrDefaultAsync(b => b.Id == businessId);

            if (business == null)
            {
                return NotFound("Business not found.");
            }

            BusinessMembers = business.UserBusinessRoles.ToList();
            BusinessOpenHours = business.OpenHours;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync([FromForm] Dictionary<string, Dictionary<string, ShiftRequirement>> shiftRequirements)
        {
            if (string.IsNullOrWhiteSpace(CalendarName) || string.IsNullOrWhiteSpace(BusinessId))
            {
                ModelState.AddModelError(string.Empty, "Calendar name and business ID are required.");
                return Page();
            }

            var business = await _context.Businesses
                .Include(b => b.OpenHours)
                .FirstOrDefaultAsync(b => b.Id == BusinessId);

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

            // Handle shift requirements
            var shifts = new List<Shift>();
            foreach (var dayEntry in shiftRequirements)
            {
                if (!int.TryParse(dayEntry.Key, out var day))
                {
                    ModelState.AddModelError(string.Empty, $"Invalid day format: {dayEntry.Key}");
                    return Page();
                }

                foreach (var timeSlotEntry in dayEntry.Value)
                {
                    if (!TimeSpan.TryParse(timeSlotEntry.Key, out var startTime))
                    {
                        ModelState.AddModelError(string.Empty, $"Invalid time format: {timeSlotEntry.Key}");
                        return Page();
                    }

                    var requirement = timeSlotEntry.Value;

                    if (requirement.MinWorkers > requirement.MaxWorkers)
                    {
                        ModelState.AddModelError(string.Empty, $"MinWorkers cannot be greater than MaxWorkers for {DaysOfWeek[day]} at {startTime}");
                        return Page();
                    }

                    shifts.Add(new Shift
                    {
                        Day = day,
                        StartTime = startTime,
                        EndTime = startTime.Add(TimeSpan.FromHours(1)),
                        MinWorkers = requirement.MinWorkers,
                        MaxWorkers = requirement.MaxWorkers,
                        CalendarId = newCalendar.Id
                    });
                }
            }

            // Save calendar and shifts to the database
            _context.Calendars.Add(newCalendar);
            _context.Shifts.AddRange(shifts);

            await _context.SaveChangesAsync();

            return RedirectToPage("/Calendars/View", new { calendarId = newCalendar.Id });
        }

        public class ShiftRequirement
        {
            public int MinWorkers { get; set; }
            public int MaxWorkers { get; set; }
        }
    }
}
