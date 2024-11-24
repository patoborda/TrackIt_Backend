namespace trackit.server.Models
{
    public class UserRequirement
    {
        public string UserId { get; set; } = null!;
        public User User { get; set; } = null!;

        public int RequirementId { get; set; }
        public Requirement Requirement { get; set; } = null!;
    }
}
