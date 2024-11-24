using System.ComponentModel.DataAnnotations;

namespace trackit.server.Dtos
{
    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        public string ClientUri { get; set; } = null!;
    }
}