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


        public ICollection<OpenHours> OpenHours { get; set; } = new List<OpenHours>();
        public List<UserBusinessRole> UserBusinessRoles { get; set; } = new List<UserBusinessRole>();
        public List<Calendar> Calendars { get; set; } = new List<Calendar>();
    }
}
