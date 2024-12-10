using BlockOut.Models;

public class ScheduleEntry
{
    public int Id { get; set; } // Primary Key
    public string CalendarId { get; set; } // Foreign Key to Calendar
    public Calendar Calendar { get; set; } // Navigation Property
    public string ParticipantId { get; set; } // Foreign Key to User
    public ApplicationUser Participant { get; set; } // Navigation Property
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
}
