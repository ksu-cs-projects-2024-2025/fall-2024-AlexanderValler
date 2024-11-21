using System;
using System.ComponentModel.DataAnnotations;

namespace BlockOut.Models
{
    public class OpenHours
    {
        public int Id { get; set; }

        [Required]
        public required string Day { get; set; } // E.g., "Monday"

        [DataType(DataType.Time)]
        public TimeSpan OpenTime { get; set; }

        [DataType(DataType.Time)]
        public TimeSpan CloseTime { get; set; }

        // Foreign key to link OpenHours to Business
        public string? BusinessId { get; set; }
        public Business? Business { get; set; }
    }
}
