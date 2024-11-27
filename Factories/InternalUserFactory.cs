using Microsoft.AspNetCore.Identity;
using trackit.server.Data;
using trackit.server.Models;

namespace trackit.server.Factories.UserFactories
{
    public class InternalUserFactory : IInternalUserFactory
    {
        private readonly UserManager<User> _userManager;
        private readonly UserDbContext _context; // Necesitamos acceso al DbContext para manejar InternalUsers

        public InternalUserFactory(UserManager<User> userManager, UserDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public User CreateUser(string email, string firstName, string lastName, string password, string cargo, string departamento)
        {
            // Crear el usuario base en AspNetUsers
            var user = new User
            {
                UserName = email, // Usamos el email como UserName
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                Image = "https://res.cloudinary.com/dvv74bwlp/image/upload/v1732681197/default-image_l5fzcd.png",
                IsEnabled = false // Opcional: Inicializa el estado del usuario
            };

            var result = _userManager.CreateAsync(user, password).Result;
            if (!result.Succeeded)
            {
                throw new Exception($"Error al crear el usuario: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            // Crear la entrada correspondiente en la tabla InternalUsers
            var internalUser = new InternalUser
            {
                Id = user.Id, // Relacionamos con el usuario base
                Cargo = cargo,
                Departamento = departamento
            };

            _context.InternalUsers.Add(internalUser);
            _context.SaveChanges(); // Guardar cambios en la base de datos

            // Asignar el rol "Interno" al usuario
            _userManager.AddToRoleAsync(user, "Interno").Wait();

            return user;
        }
    }
}
