using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlockOut.Models
{
    public class UserBusinessCalendar
    {
        [Key, Column(Order = 0)]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Key, Column(Order = 1)]
        public string CalendarId { get; set; }
        public Calendar Calendar { get; set; }

        [Key, Column(Order = 2)]
        public string BusinessId { get; set; }
        public Business Business { get; set; }

        public string Type { get; set; } // "Preferences" or "Availability"
    }
}