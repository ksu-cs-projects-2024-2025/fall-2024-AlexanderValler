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

        private readonly ILogger<ShiftsModel> _logger;

        public ShiftsModel(ApplicationDbContext context, ILogger<ShiftsModel> logger)
        {
            _context = context;
            _logger = logger;
        }


        [BindProperty]
        public Shift Shift { get; set; }

        [BindProperty(SupportsGet = true)]
        public string BusinessId { get; set; }

        public string CalendarId { get; set; } // Foreign Key to Calendar (Optional if not used)


        [BindProperty]
        public List<ShiftHourRequirement> HourlyRequirements { get; set; }

        public List<Shift> Shifts { get; set; }
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

            // Try to find an existing CalendarId for the business, or initialize a default
            var calendar = await _context.Calendars
                .FirstOrDefaultAsync(c => c.BusinessId == businessId);

            CalendarId = calendar?.Id ?? ""; // Use an empty string if no Calendar exists

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

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                foreach (var state in ModelState)
                {
                    if (state.Value.Errors.Count > 0)
                    {
                        foreach (var error in state.Value.Errors)
                        {
                            _logger.LogWarning($"Key: {state.Key}, Error: {error.ErrorMessage}");
                        }
                    }
                }
                return Page();
            }

            _logger.LogInformation("Processing POST request for Business ID: {BusinessId}", BusinessId);

            // Retrieve the Business entity using the BusinessId
            var business = await _context.Businesses.FindAsync(BusinessId);
            if (business == null)
            {
                _logger.LogError("Business with ID {BusinessId} not found.", BusinessId);
                ModelState.AddModelError("Shift.Business", "Invalid Business.");
                return Page();
            }

            // Handle CalendarId: Create new or retrieve existing
            if (string.IsNullOrWhiteSpace(Shift.CalendarId))
            {
                var newCalendar = new Calendar
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Default Calendar",
                    BusinessId = BusinessId
                };

                _context.Calendars.Add(newCalendar);
                await _context.SaveChangesAsync();

                Shift.CalendarId = newCalendar.Id; // Assign the new CalendarId
            }

            // Assign Business to Shift
            Shift.Business = business;

            // Parse Hourly Requirements
            var hourlyRequirements = new List<ShiftHourRequirement>();
            foreach (var dayIndex in Enumerable.Range(0, 7))
            {
                var entryCount = Request.Form.Keys.Count(k => k.StartsWith($"HourlyRequirements[{dayIndex}]")) / 4; // Each entry has 4 fields
                for (var i = 0; i < entryCount; i++)
                {
                    var startTimeKey = $"HourlyRequirements[{dayIndex}][{i}][HourStartTime]";
                    var endTimeKey = $"HourlyRequirements[{dayIndex}][{i}][HourEndTime]";
                    var minWorkersKey = $"HourlyRequirements[{dayIndex}][{i}][MinWorkers]";
                    var maxWorkersKey = $"HourlyRequirements[{dayIndex}][{i}][MaxWorkers]";

                    if (Request.Form.TryGetValue(startTimeKey, out var startTime) &&
                        Request.Form.TryGetValue(endTimeKey, out var endTime) &&
                        Request.Form.TryGetValue(minWorkersKey, out var minWorkers) &&
                        Request.Form.TryGetValue(maxWorkersKey, out var maxWorkers))
                    {
                        hourlyRequirements.Add(new ShiftHourRequirement
                        {
                            HourStartTime = TimeSpan.Parse(startTime),
                            HourEndTime = TimeSpan.Parse(endTime),
                            MinWorkers = int.Parse(minWorkers),
                            MaxWorkers = int.Parse(maxWorkers),
                        });
                    }
                }
            }

            // Assign hourly requirements
            Shift.HourlyRequirements = hourlyRequirements;

            // Add and save the Shift
            _context.Shifts.Add(Shift);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Shift created successfully for Business ID: {BusinessId}", BusinessId);
            return RedirectToPage("/Calendars/CreateCalendar", new { businessId = BusinessId });
        }


    }
}
