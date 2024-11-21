namespace trackit.server.Dtos
{
    public class RequirementResponseDto
    {
        public int Id { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public string Code { get; set; }
        public string RequirementType { get; set; }
        public string Category { get; set; }
        public string Priority { get; set; } // Nombre de la prioridad
        public DateTime Date { get; set; } // Fecha de creación
        public string Status { get; set; } // Estado del requerimiento
    }
}
