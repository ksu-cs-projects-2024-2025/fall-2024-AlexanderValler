namespace BlockOut.Models
{
    public class UserBusinessRole
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public string BusinessId { get; set; }
        public Business Business { get; set; }

        public string Role { get; set; } // E.g., "Owner", "Manager", "Employee"
    }
}
