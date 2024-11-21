using System;
using System.Collections.Generic;
using System;
using System.Collections.Generic;


namespace trackit.server.Models
{
    public class Requirement
    {
        public int Id { get; set; }
        public string Subject { get; set; }
        public string Code { get; set; } // Optional: Auto-generated
        public string Description { get; set; }

        public int RequirementTypeId { get; set; }
        public RequirementType RequirementType { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public ICollection<RequirementRelation> RelatedRequirements { get; set; }
        /*
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }

        public int PriorityId { get; set; }
        public Priority Priority { get; set; }
        public string Status { get; set; }

        // Relationship with related requirements
      
       */
    }
}
