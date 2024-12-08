using BlockOut.Data;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace BlockOut.Models
{
    public class Business
    {
        public string Id { get; set; }
        public string Name { get; set; }



        // Collection of calendars associated with this business
        public List<Calendar> Calendars { get; set; } = new List<Calendar>();

        // Collection of open hours
        public List<OpenHours> OpenHours { get; set; } = new List<OpenHours>();

        // Collection of user roles in this business
        public List<UserBusinessRole> UserBusinessRoles { get; set; } = new List<UserBusinessRole>();

        // Collection of UserBusinessCalendars
        public List<UserBusinessCalendar> UserBusinessCalendars { get; set; } = new List<UserBusinessCalendar>();
    }
}
