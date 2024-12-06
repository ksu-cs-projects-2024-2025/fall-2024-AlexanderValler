﻿using BlockOut.Data;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace BlockOut.Models
{
    public class Business
    {

        // Default constructor
        public Business()
        {
            OpenHours = new List<OpenHours>();
            UserBusinessRoles = new List<UserBusinessRole>();
            Calendars = new List<Calendar>();
        }


        public string? Id { get; set; }

        public string? Name { get; set; }

        // Collection of OpenHours for each day of the week
        public List<OpenHours> OpenHours { get; set; } = new List<OpenHours>();

        // Collection to support many-to-many relationship with ApplicationUser through UserBusinessRole
        public List<UserBusinessRole> UserBusinessRoles { get; set; } = new List<UserBusinessRole>();

        // One-to-many relationship with Calendar
        public List<Calendar>? Calendars { get; set; } = new List<Calendar>();

    }
}
