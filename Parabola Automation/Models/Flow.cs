namespace Parabola_Automation.Models
{
    public class Flow
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation property: This will let you get the users that have access to this flow
        public ICollection<UserFlow> UserFlows { get; set; } = new List<UserFlow>();
    }
}
