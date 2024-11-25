namespace trackit.server.Dtos
{
    public class UpdateUserStatusDto
    {
        public string Email { get; set; }
        public bool IsEnabled { get; set; }  // Estado de habilitación o deshabilitación
    }
}
