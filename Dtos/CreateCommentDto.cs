namespace trackit.server.Dtos
{
    public class CreateCommentDto
    {
        public string Subject { get; set; }
        public string Description { get; set; }
        public int RequirementId { get; set; }
        public string UserId { get; set; }
        public List<string> Files { get; set; }
    }
}
