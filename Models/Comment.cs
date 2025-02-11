public class Comment
{
    public int Id { get; set; }
    public int RequirementId { get; set; }
    public string UserName { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Relación con archivos adjuntos (Cada comentario puede tener archivos)
    public List<CommentAttachment> Attachments { get; set; } = new List<CommentAttachment>();

    // Relación con el requerimiento
    public Requirement Requirement { get; set; }
}
