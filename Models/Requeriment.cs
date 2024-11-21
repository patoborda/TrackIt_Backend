using System;
using System.Collections.Generic;

namespace trackit.server.Models
{
    public class Requirement
    {
        public int Id { get; set; }
        public string Subject { get; set; }
        public string Code { get; set; } // Código auto-generado
        public string Description { get; set; }

        // Relación con RequirementType
        public int RequirementTypeId { get; set; }
        public RequirementType RequirementType { get; set; }

        // Relación con Category
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        // Relación con Priority
        public int PriorityId { get; set; }
        public Priority Priority { get; set; }

        // Fecha de creación (valor predeterminado: ahora)
        public DateTime Date { get; set; } = DateTime.UtcNow;

        // Estado del requerimiento (valor predeterminado: "Abierto")
        public string Status { get; set; } = "Abierto";

        // Relación con otros requerimientos
        public ICollection<RequirementRelation> RelatedRequirements { get; set; }
    }
}
