using Microsoft.AspNetCore.Identity;
using trackit.server.Models;
using trackit.server.Repositories.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using trackit.server.Exceptions;

namespace trackit.server.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<User> _userManager;

        public UserRepository(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // Lanza una excepción que será capturada por el middleware
                throw new UserNotFoundException("User not found with the provided email.");
            }
            return user;
        }


        public async Task<bool> CreateUserAsync(User user, string password)
        {
            var result = await _userManager.CreateAsync(user, password);
            return result.Succeeded;
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<IList<string>> GetUserRolesAsync(User user)
        {
            return await _userManager.GetRolesAsync(user);
        }
    }
}
