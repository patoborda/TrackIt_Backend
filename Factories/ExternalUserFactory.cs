using Microsoft.AspNetCore.Identity;
using trackit.server.Models;

namespace trackit.server.Factories.UserFactories
{
    public class ExternalUserFactory : IExternalUserFactory
    {
        private readonly UserManager<User> _userManager;

        public ExternalUserFactory(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public User CreateUser(string email, string firstName, string lastName, string password, string cuil, string empresa, string descripcion)
        {
            var user = new ExternalUser
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                Cuil = cuil,
                Empresa = empresa,
                Descripcion = descripcion
            };

            var result = _userManager.CreateAsync(user, password).Result;
            if (result.Succeeded)
            {
                _userManager.AddToRoleAsync(user, "Externo").Wait();
            }
            else
            {
                throw new Exception($"Error al crear el usuario: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            return user;
        }
    }
}
