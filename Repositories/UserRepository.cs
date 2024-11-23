using Microsoft.AspNetCore.Identity;
using trackit.server.Models;
using trackit.server.Repositories.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using trackit.server.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace trackit.server.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserRepository(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
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

        public async Task<bool> RoleExistsAsync(string roleName)
        {
            return await _roleManager.RoleExistsAsync(roleName);
        }

        public async Task AssignRoleAsync(User user, string roleName)
        {
            var result = await _userManager.AddToRoleAsync(user, roleName);
            if (!result.Succeeded)
            {
                throw new Exception($"Error assigning role {roleName} to user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }

        public async Task DeleteUserAsync(User user)
        {
            await _userManager.DeleteAsync(user);
        }

        public async Task<string> GeneratePasswordResetTokenAsync(User user)
        {
            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }

        public async Task<bool> ResetPasswordAsync(User user, string token, string newPassword)
        {
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            return result.Succeeded;
        }


        /*****************************************************************************************************************/


        public async Task<User> GetUserWithRelationsByIdAsync(string userId)
        {
            return await _userManager.Users
                .Include(u => u.AdminUser)
                .Include(u => u.InternalUser)
                .Include(u => u.ExternalUser)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }




    }

}
