namespace trackit.server.Dtos
{
    public class InternalUserProfileDto : UserProfileDto
    {
        public string Cargo { get; set; } = null!;
        public string Departamento { get; set; } = null!;
        public string Role { get; set; }
    }

}
