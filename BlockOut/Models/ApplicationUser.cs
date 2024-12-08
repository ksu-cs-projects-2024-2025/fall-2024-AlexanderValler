using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace BlockOut.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? JobTitle { get; set; }
        public int? ProfilePictureId { get; set; }

        public Calendar? PreferencesCalendar { get; set; }
        public Calendar? AvailabilityCalendar { get; set; }
        // Relationships for Business Roles
        public List<UserBusinessRole> UserBusinessRoles { get; set; } = new List<UserBusinessRole>();

        // Calendar Preferences Per Business
        public List<UserBusinessCalendar> UserBusinessCalendars { get; set; } = new List<UserBusinessCalendar>();
    }
}

