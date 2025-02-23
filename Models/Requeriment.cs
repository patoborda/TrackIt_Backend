using trackit.server.Models;

public class Requirement
{
    public int Id { get; set; }
    public string Subject { get; set; }
    public string Code { get; set; }
    public string Description { get; set; }

    public bool IsDeleted { get; set; } = false; // Eliminación lógica
    public DateTime? RestoredAt { get; set; } // Fecha de restauración

    public int RequirementTypeId { get; set; }
    public RequirementType RequirementType { get; set; }

    public int CategoryId { get; set; }
    public Category Category { get; set; }

    public int? PriorityId { get; set; }
    public Priority? Priority { get; set; }

    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Abierto";

    public string CreatedByUserId { get; set; } // Usuario creador
    public User CreatedByUser { get; set; } // Relación con el usuario



    // Relación con archivos adjuntos (de los requerimientos)
    public ICollection<Attachment> Attachments { get; set; }

    public ICollection<RequirementRelation> RelatedRequirements { get; set; } = new List<RequirementRelation>();
    public ICollection<UserRequirement> UserRequirements { get; set; } = new List<UserRequirement>();
}
