using System;
using System.ComponentModel.DataAnnotations;

namespace BlockOut.Models
{
    public class OpenHours
    {
        public int Id { get; set; }

        [Required]
        public required int Day { get; set; } // 1 = Monday, 2 = Tuesday, ..., 7 = Sunday

        [DataType(DataType.Time)]
        public TimeSpan? OpenTime { get; set; } // Null if the business is closed for the day

        [DataType(DataType.Time)]
        public TimeSpan? CloseTime { get; set; } // Null if the business is closed for the day

        // A simple flag to indicate if the business is closed all day
        public bool IsClosed { get; set; }

        // Foreign key to link OpenHours to Business
        public string? BusinessId { get; set; }
        public Business? Business { get; set; }
    }
}
