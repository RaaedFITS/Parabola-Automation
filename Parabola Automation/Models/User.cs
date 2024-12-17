namespace Parabola_Automation.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string Role { get; set; } = null!; // "admin" or "employee"
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation property: This will let you easily get the flows the user has
        public ICollection<UserFlow> UserFlows { get; set; } = new List<UserFlow>();
    }
}
