﻿using Microsoft.AspNetCore.Identity;

namespace trackit.server.Models
{
    public class User : IdentityUser
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public bool IsEnabled { get; set; } = false; // Estado inicial deshabilitado

        // Relaciones uno a uno
        public InternalUser? InternalUser { get; set; }
        public ExternalUser? ExternalUser { get; set; }
        public AdminUser? AdminUser { get; set; }
    }

    // Clase para datos específicos del usuario interno
    public class InternalUser
    {
        public string Id { get; set; } = null!; // Clave primaria igual al Id de User
        public required string Cargo { get; set; }
        public required string Departamento { get; set; }

        // Relación inversa con User
        public User User { get; set; } = null!;
    }

    // Clase para datos específicos del usuario externo
    public class ExternalUser
    {
        public string Id { get; set; } = null!; // Clave primaria igual al Id de User
        public required string Cuil { get; set; }
        public required string Empresa { get; set; }
        public required string Descripcion { get; set; }

        // Relación inversa con User
        public User User { get; set; } = null!;
    }

    // Clase para datos específicos del administrador
    public class AdminUser
    {
        public string Id { get; set; } = null!; // Clave primaria igual al Id de User

        // Relación inversa con User
        public User User { get; set; } = null!;
    }
}
