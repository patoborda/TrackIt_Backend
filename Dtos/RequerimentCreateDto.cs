using System.ComponentModel.DataAnnotations;

public class RequirementCreateDto
{
    [Required]
    public string Subject { get; set; }

    [Required]
    public int RequirementTypeId { get; set; }

    [Required]
    public int CategoryId { get; set; }

    public string? Description { get; set; }

    public int? PriorityId { get; set; }

    public List<string>? AssignedUsers { get; set; } // Lista de IDs de usuarios asignados

    public List<int>? RelatedRequirementIds { get; set; } // Opcional
}
