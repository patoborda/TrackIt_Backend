public class CommentAttachment
{
    public int Id { get; set; }
    public int CommentId { get; set; }
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // Relación con el comentario
    public Comment Comment { get; set; }
}
