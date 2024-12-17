namespace Parabola_Automation.Models
{
    public class UserFlow
    {
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int FlowId { get; set; }
        public Flow Flow { get; set; } = null!;

        public DateTime AddedAt { get; set; }
    }
}
