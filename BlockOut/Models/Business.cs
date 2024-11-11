using System.ComponentModel.DataAnnotations;

namespace BlockOut.Models
{
    public class Business
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        public string UserId { get; set; } // Associates the business with a specific user

        [Required]
        public string Role { get; set; } // "Owner", "Manager", or "Employee"
    }
}
