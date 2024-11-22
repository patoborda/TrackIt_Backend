namespace trackit.server.Models
{
    public class RequirementActionLog
    {
        public int Id { get; set; }
        public int RequirementId { get; set; }
        public Requirement Requirement { get; set; }
        public string Action { get; set; } // Acción realizada ("Modificado", "Comentario Agregado", etc.)
        public string PerformedBy { get; set; } // Usuario que realizó la acción
        public DateTime Timestamp { get; set; } = DateTime.UtcNow; // Fecha y hora
        public string Details { get; set; } // Información sobre los cambios (JSON con "antes" y "después")
    }
}
