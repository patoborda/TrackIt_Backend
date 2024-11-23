using Microsoft.AspNetCore.Identity;

namespace trackit.server.Models
{
    public class User : IdentityUser
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public bool IsEnabled { get; set; } = false; // Estado inicial deshabilitado
}

    // Usuario admin heredado de User
    public class AdminUser : User
    {
        // La clase AdminUser puede tener un comportamiento específico
        public AdminUser()
        {
            IsEnabled = true; // Admin siempre habilitado
        }
    }
    public class InternalUser : User
    {
        public required string Cargo { get; set; }
        public required string Departamento { get; set; }
    }

    public class ExternalUser : User
    {
        public required string Cuil { get; set; }
        public required string Empresa { get; set; }
        public required string Descripcion { get; set; }
    }

}

