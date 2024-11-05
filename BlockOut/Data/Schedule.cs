using System;
using System.ComponentModel.DataAnnotations;

namespace BlockOut.Models
{
    public class Schedule
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } // Associates the schedule with a specific user

        [Required]
        [MaxLength(100)]
        public string Title { get; set; } // Title of the schedule

        [Required]
        public DateTime Date { get; set; } // Date of the schedule

        [Required]
        public TimeSpan Time { get; set; } // Time of the schedule

        [Required]
        [MaxLength(100)]
        public string GroupName { get; set; } // Represents the business or schedule group name
    }
}
