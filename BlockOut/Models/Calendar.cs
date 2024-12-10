using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace BlockOut.Models
{
    public class Calendar
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Ensure the ID is auto-generated
        public string Id { get; set; }
        public string Name { get; set; }
        public string? Type { get; set; } // "Weekly", "Monthly", etc.
        public string? Data { get; set; } // Serialized calendar data

        // Link to Business
        public string? BusinessId { get; set; }
        public Business? Business { get; set; }

        // Availability and Preferences
        public string? AvailabilityUserId { get; set; }
        public ApplicationUser? AvailabilityUser { get; set; }
        public string? PreferencesUserId { get; set; }
        public ApplicationUser? PreferencesUser { get; set; }


        // Many-to-Many with Users
        public List<UserBusinessCalendar> UserBusinessCalendars { get; set; } = new List<UserBusinessCalendar>();
    }
}
