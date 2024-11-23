using Microsoft.AspNetCore.Identity;
using trackit.server.Data;
using trackit.server.Models;

namespace trackit.server.Factories.UserFactories
{
    public class ExternalUserFactory : IExternalUserFactory
    {
        private readonly UserManager<User> _userManager;
        private readonly UserDbContext _context; // Necesitamos el DbContext para manejar ExternalUsers

        public ExternalUserFactory(UserManager<User> userManager, UserDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public User CreateUser(string email, string firstName, string lastName, string password, string cuil, string empresa, string descripcion)
        {
            // Crear el usuario base en AspNetUsers
            var user = new User
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                IsEnabled = false // Opcional: Inicializa el estado del usuario
            };

            var result = _userManager.CreateAsync(user, password).Result;
            if (!result.Succeeded)
            {
                throw new Exception($"Error al crear el usuario: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            // Crear la entrada correspondiente en la tabla ExternalUsers
            var externalUser = new ExternalUser
            {
                Id = user.Id, // Relacionamos con el usuario base
                Cuil = cuil,
                Empresa = empresa,
                Descripcion = descripcion
            };

            _context.ExternalUsers.Add(externalUser);
            _context.SaveChanges(); // Guardar cambios en la base de datos

            // Asignar el rol "Externo" al usuario
            _userManager.AddToRoleAsync(user, "Externo").Wait();

            return user;
        }
    }
}
