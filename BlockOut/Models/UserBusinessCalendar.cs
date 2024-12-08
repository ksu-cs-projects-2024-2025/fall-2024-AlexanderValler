using System.ComponentModel.DataAnnotations;

namespace BlockOut.Models
{
    public class UserBusinessCalendar
    {
        [Key]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Key]
        public string CalendarId { get; set; }
        public Calendar Calendar { get; set; }

        [Key]
        public string BusinessId { get; set; }
        public Business Business { get; set; }

        public string Type { get; set; } // "Preferences" or "Availability"
    }
}
