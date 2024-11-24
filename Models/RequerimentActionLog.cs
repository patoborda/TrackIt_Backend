public class RequirementActionLog
{
    public int Id { get; set; }
    public int RequirementId { get; set; }
    public string Action { get; set; } // Ejemplo: "Creado", "Modificado", "Asignado"
    public string Details { get; set; } // Detalles específicos de la acción
    public DateTime Timestamp { get; set; }
    public string PerformedByUserId { get; set; }
}
