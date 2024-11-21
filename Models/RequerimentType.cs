using System.Collections.Generic;

namespace trackit.server.Models
{
    public class RequirementType
    {
       
        public int Id { get; set; }
        public string Name { get; set; } // Example: Hardware, Software, Maintenance
        public ICollection<Category> Categories { get; set; } // One-to-many relationship with Category
     
    }
}
