using BlockOut.Models;
using System;
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
        public Calendar? Calendar { get; set; } // Nullable Navigation Property

        // Hourly Requirements for each hour within the shift
        public List<ShiftHourRequirement> HourlyRequirements { get; set; } = new List<ShiftHourRequirement>();

        public int TotalWeekHours { get; set; }
        public int MaxWeeklyHoursPerPerson { get; set; }
        public int MaxDailyHoursPerPerson { get; set; }

        public string BusinessId { get; set; } // Foreign Key to Business
        public Business? Business { get; set; } // Nullable Navigation Property
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
