using System.ComponentModel.DataAnnotations;

namespace trackit.server.Dtos
{
    public class RegisterUserDto
    {
        [Required(ErrorMessage = "First name is required")]
        public required string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        public required string LastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public required string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public required string ConfirmPassword { get; set; }

        // Para usuarios internos
         public class RegisterInternalUserDto : RegisterUserDto
        {
            [Required(ErrorMessage = "Cargo is required")]
            public required string Cargo { get; set; }

            [Required(ErrorMessage = "Departamento is required")]
            public required string Departamento { get; set; }
        }

        // Para usuarios externos
        public class RegisterExternalUserDto : RegisterUserDto
        {
            [Required(ErrorMessage = "Cuil is required")]
            public required string Cuil { get; set; }

            [Required(ErrorMessage = "Empresa is required")]
            public required string Empresa { get; set; }

            [Required(ErrorMessage = "Descripcion is required")]
            public required string Descripcion { get; set; }
        }
    }
}
