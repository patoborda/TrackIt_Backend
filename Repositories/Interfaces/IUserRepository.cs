using trackit.server.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace trackit.server.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetUserByEmailAsync(string email);
        Task<User> GetUserByIdAsync(string userId);
        Task<bool> CreateUserAsync(User user, string password);
        Task<bool> UpdateUserAsync(User user);
        Task<IList<string>> GetUserRolesAsync(User user);
        Task<bool> RoleExistsAsync(string roleName);  // Verifica si un rol existe
        Task AssignRoleAsync(User user, string roleName);  // Asigna un rol a un usuario
        Task DeleteUserAsync(User user);  // Elimina un usuario
        Task<string> GeneratePasswordResetTokenAsync(User user);  // Genera token de restablecimiento
        Task<IdentityResult> ResetPasswordAsync(User user, string token, string newPassword);  // Restablece contraseña
        Task<User> GetUserWithRelationsByIdAsync(string userId);
        Task<List<User>> GetUsersExcludingAdminsAsync();
        Task<List<User>> GetExternalUsersAsync();
        Task<List<User>> GetInternalUsersAsync();
        Task AssignDefaultImageToAllUsersAsync();
        Task<User> UpdateUserImageAsync(string userId, string imageUrl);
        Task<List<User>> GetAssignedUsersAsync(int requirementId);

    }

}
