using Microsoft.AspNetCore.Identity;
using trackit.server.Models;

namespace trackit.server.Factories.UserFactories
{
    public class InternalUserFactory : IInternalUserFactory
    {
        private readonly UserManager<User> _userManager;

        public InternalUserFactory(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public User CreateUser(string email, string firstName, string lastName, string password, string cargo, string departamento)
        {
            var user = new InternalUser
            {
                UserName = email, // Usamos el email como UserName
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                Cargo = cargo, // Puedes inicializar con valores por defecto
                Departamento = cargo
            };

            var result = _userManager.CreateAsync(user, password).Result;
            if (result.Succeeded)
            {
                _userManager.AddToRoleAsync(user, "Interno").Wait();
            }
            else
            {
                throw new Exception($"Error al crear el usuario: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            return user;
        }
    }
}
