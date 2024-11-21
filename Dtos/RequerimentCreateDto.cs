namespace trackit.server.Dtos
{
    public class RequirementCreateDto
    {
        public string Subject { get; set; }
        public string Description { get; set; }
        public int RequirementTypeId { get; set; } // Foreign key to RequirementType
        public int CategoryId { get; set; } // Foreign key to Category
    }
}
