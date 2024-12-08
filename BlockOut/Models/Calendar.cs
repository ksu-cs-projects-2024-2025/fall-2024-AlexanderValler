using BlockOut.Models;

namespace BlockOut.Models
{
    public class Calendar
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; } // "Weekly", "Monthly", etc.
        public string Data { get; set; } // Serialized calendar data

        // Link to Business
        public string BusinessId { get; set; }
        public Business Business { get; set; }

        // Users tied to this calendar
        public List<UserBusinessCalendar> UserBusinessCalendars { get; set; } = new List<UserBusinessCalendar>();


        // Availability and Preferences
        public string? AvailabilityUserId { get; set; }
        public ApplicationUser? AvailabilityUser { get; set; }
        public string? PreferencesUserId { get; set; }
        public ApplicationUser? PreferencesUser { get; set; }
    }
}
