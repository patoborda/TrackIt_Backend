namespace trackit.server.Dtos
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int RequirementTypeId { get; set; } // Relación con RequirementType
    }
}
