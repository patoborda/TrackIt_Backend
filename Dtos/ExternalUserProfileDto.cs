namespace trackit.server.Dtos
{
    public class ExternalUserProfileDto : UserProfileDto
    {
        public string Cuil { get; set; } = null!;
        public string Empresa { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
    }

}
