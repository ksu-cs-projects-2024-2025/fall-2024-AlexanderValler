using BlockOut.Models;

public class Calendar
{
    public int Id { get; set; }
    public string Name { get; set; } // "AvailabilityCalendar" or "PreferencesCalendar"
    public string? Type { get; set; } // Type of calendar, e.g., weekly, monthly
    public string? Data { get; set; } // Placeholder for serialized calendar data

    // Separate foreign keys for AvailabilityCalendar and PreferencesCalendar
    public string? AvailabilityUserId { get; set; }
    public string? PreferencesUserId { get; set; }

    // Navigation properties for ApplicationUser
    public ApplicationUser? AvailabilityUser { get; set; }
    public ApplicationUser? PreferencesUser { get; set; }

    public int? BusinessId { get; set; }
    public Business? Business { get; set; }

}
