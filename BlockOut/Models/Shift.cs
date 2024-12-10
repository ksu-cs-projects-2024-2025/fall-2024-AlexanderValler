using System;
using System.Collections.Generic;

namespace BlockOut.Models
{
    public class Shift
    {
        public int Id { get; set; } // Primary Key
        public int Day { get; set; } // Day of the week (1 = Sunday, 7 = Saturday)
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string CalendarId { get; set; } // Foreign Key to Calendar
        public Calendar Calendar { get; set; } // Navigation Property

        // Hourly Requirements for each hour within the shift
        public List<ShiftHourRequirement> HourlyRequirements { get; set; } = new List<ShiftHourRequirement>();

        // Total hours to be distributed across all shifts and workers for the entire week
        public int TotalWeekHours { get; set; }

        // Maximum hours a single person can work for the entire week
        public int MaxWeeklyHoursPerPerson { get; set; }

        // Maximum hours a single person can work in a single day
        public int MaxDailyHoursPerPerson { get; set; }
    }

    public class ShiftHourRequirement
    {
        public int Id { get; set; } // Primary Key
        public int ShiftId { get; set; } // Foreign Key to Shift
        public TimeSpan HourStartTime { get; set; } // E.g., 9:00 AM
        public TimeSpan HourEndTime { get; set; } // E.g., 10:00 AM
        public int MinWorkers { get; set; }
        public int MaxWorkers { get; set; }

        public Shift Shift { get; set; } // Navigation Property
    }
}
