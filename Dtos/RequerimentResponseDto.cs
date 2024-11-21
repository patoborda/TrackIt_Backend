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

        /*
         public string Status { get; set; }
         public string Priority { get; set; }
         public DateTime Date { get; set; }
       */

    }
}
