namespace trackit.server.Models
{
    public class RequirementRelation
    {
        public int RequirementId { get; set; }
        public Requirement Requirement { get; set; }

        public int RelatedRequirementId { get; set; }
        public Requirement RelatedRequirement { get; set; }
    }
}
