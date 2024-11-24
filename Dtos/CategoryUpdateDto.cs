namespace trackit.server.Dtos
{
    public class CategoryUpdateDto
    {
        public string Name { get; set; }
        public int RequirementTypeId { get; set; } // Relación con RequirementType
    }
}
