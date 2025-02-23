using Microsoft.AspNetCore.Identity;

namespace trackit.server.Models
{
    public class User : IdentityUser
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public bool IsEnabled { get; set; } = false; // Estado inicial deshabilitado
        public string? Image { get; set; }

        // Relaciones uno-a-uno
        public InternalUser? InternalUser { get; set; }
        public ExternalUser? ExternalUser { get; set; }
        public AdminUser? AdminUser { get; set; }

        // Relación muchos-a-muchos con Requirements
        public ICollection<UserRequirement> UserRequirements { get; set; } = new List<UserRequirement>();

        // Relación muchos-a-muchos con Notifications
        public ICollection<UserNotification> UserNotifications { get; set; } = new List<UserNotification>();

        // Relación uno-a-muchos con Comments
        public List<Comment> Comments { get; set; } = new List<Comment>();
    }
    public class InternalUser
    {
        public string Id { get; set; } = null!; // Clave primaria igual al Id de User
        public required string Cargo { get; set; }
        public required string Departamento { get; set; }
        public User User { get; set; } = null!;
    }

    public class ExternalUser
    {
        public string Id { get; set; } = null!; // Clave primaria igual al Id de User
        public required string Cuil { get; set; }
        public required string Empresa { get; set; }
        public required string Descripcion { get; set; }
        public User User { get; set; } = null!;
    }

    public class AdminUser
    {
        public string Id { get; set; } = null!; // Clave primaria igual al Id de User

        // Relación inversa con User
        public User User { get; set; } = null!;
    }


}
