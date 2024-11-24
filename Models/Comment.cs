using trackit.server.Models;

public class Comment
{
    public int Id { get; set; }
    public string Subject { get; set; }
    public string Description { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan Time { get; set; }

    // Relación con Requirement
    public int RequirementId { get; set; }  // Asegúrate de que sea int
    public Requirement Requirement { get; set; }

    // Relación con User
    public string UserId { get; set; }
    public User User { get; set; }

    // Archivos adjuntos
    public List<AttachedFile> Files { get; set; } = new();
}
