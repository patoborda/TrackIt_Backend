namespace trackit.server.Dtos
{
    public class AdminUserProfileDto : UserProfileDto
    {
        public string AdminSpecificAttribute { get; set; } = null!; // Ejemplo
        public string Role { get; set; }
    }

}
