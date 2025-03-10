﻿public class Attachment
{
    public int Id { get; set; }
    public int RequirementId { get; set; }
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public Requirement Requirement { get; set; }
}
