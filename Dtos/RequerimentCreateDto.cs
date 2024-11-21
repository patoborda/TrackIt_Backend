namespace trackit.server.Dtos
{
    public class RequirementCreateDto
    {
        public string Subject { get; set; }
        public string Description { get; set; }
        public int RequirementTypeId { get; set; }
        public int CategoryId { get; set; }
        public int PriorityId { get; set; }
        public List<int>? RelatedRequirementIds { get; set; } // IDs de requerimientos relacionados
    }
}
