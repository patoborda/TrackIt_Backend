public class ChatMessage
{
    public int Id { get; set; }
    public int RequirementId { get; set; } // Asumiendo que el id del requerimiento es un entero
    public string UserName { get; set; }
    public string Message { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
