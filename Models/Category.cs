namespace trackit.server.Models
{
    public class Category
    {
       
        public int Id { get; set; }
        public string Name { get; set; } // Example: Database Software Service
        public int RequirementTypeId { get; set; }
        public RequirementType RequirementType { get; set; }
       
    }
}
