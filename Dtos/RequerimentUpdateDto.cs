namespace trackit.server.Dtos
{
    public class RequirementUpdateDto
    {
        public string Subject { get; set; } // Nuevo título
        public string Description { get; set; } // Nueva descripción
        public int? PriorityId { get; set; } // Nueva prioridad (opcional)
        public string Status { get; set; } // Nuevo estado
    }
}
