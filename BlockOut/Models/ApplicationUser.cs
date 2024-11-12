using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace BlockOut.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? JobTitle { get; set; }
        public int? ProfilePictureId { get; set; }

        public Calendar? AvailabilityCalendar { get; set; }
        public Calendar? PreferencesCalendar { get; set; }

        // Add this line to support the many-to-many relationship
        public List<UserBusinessRole> UserBusinessRoles { get; set; } = new List<UserBusinessRole>();
    }
}
