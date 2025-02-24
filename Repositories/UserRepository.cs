using Microsoft.AspNetCore.Identity;
using trackit.server.Models;
using trackit.server.Repositories.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using trackit.server.Exceptions;
using Microsoft.EntityFrameworkCore;
using trackit.server.Data;

namespace trackit.server.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserDbContext _userDbContext;

        public UserRepository(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, UserDbContext userDbContext)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _userDbContext = userDbContext;
        }

        public async Task<User> GetUserByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
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

        public async Task<bool> DeleteUserAsync(User user)
        {
            // Cargar explícitamente las relaciones que pueden causar conflicto.
            await _userDbContext.Entry(user).Reference(u => u.ExternalUser).LoadAsync();
            await _userDbContext.Entry(user).Reference(u => u.InternalUser).LoadAsync();
            await _userDbContext.Entry(user).Reference(u => u.AdminUser).LoadAsync();

            // Remover las entidades relacionadas, si existen.
            if (user.ExternalUser != null)
            {
                _userDbContext.ExternalUsers.Remove(user.ExternalUser);
            }
            if (user.InternalUser != null)
            {
                _userDbContext.InternalUsers.Remove(user.InternalUser);
            }
            if (user.AdminUser != null)
            {
                _userDbContext.AdminUsers.Remove(user.AdminUser);
            }

            // Remover el usuario
            _userDbContext.Users.Remove(user);

            // Guardar los cambios y retornar true si se eliminaron registros.

            var changes = await _userDbContext.SaveChangesAsync();
            return changes > 0;
        }

        public async Task<string> GeneratePasswordResetTokenAsync(User user)
        {
            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }

        public async Task<IdentityResult> ResetPasswordAsync(User user, string token, string newPassword)
        {
            return await _userManager.ResetPasswordAsync(user, token, newPassword);
        }

        public async Task<User> GetUserWithRelationsByIdAsync(string userId)
        {
            var user = await _userManager.Users
                .Include(u => u.AdminUser)
                .Include(u => u.InternalUser)
                .Include(u => u.ExternalUser)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new UserNotFoundException("User not found with the provided UserId.");

            return user;
        }

        public async Task<List<User>> GetUsersExcludingAdminsAsync()
        {
            // Obtener usuarios, excluyendo los administradores (usuarios con relación a AdminUser)
            return await _userManager.Users
                .Where(u => u.AdminUser == null)  // Excluir administradores (no deben tener relación con AdminUser)
                .Include(u => u.InternalUser) // Incluir relaciones de usuarios internos
                .Include(u => u.ExternalUser) // Incluir relaciones de usuarios externos
                .ToListAsync();
        }

        // Método para obtener solo los usuarios externos habilitados
        public async Task<List<User>> GetExternalUsersAsync()
        {
            return await _userManager.Users
                .Where(u => u.ExternalUser != null)  // Solo externos habilitados
                .Include(u => u.ExternalUser)  // Incluir relación con ExternalUser
                .ToListAsync();
        }

        // Método para obtener solo los usuarios internos habilitados
        public async Task<List<User>> GetInternalUsersAsync()
        {
            return await _userManager.Users
                .Where(u => u.InternalUser != null)  // Solo internos habilitados
                .Include(u => u.InternalUser)  // Incluir relación con InternalUser
                .ToListAsync();
        }

        // Asignar una imagen predeterminada a todos los usuarios que no tienen imagen
        public async Task AssignDefaultImageToAllUsersAsync()
        {
            var usersWithoutImage = await _userDbContext.Users
                .Where(u => string.IsNullOrEmpty(u.Image)) // Filtrar usuarios sin imagen
                .ToListAsync();

            foreach (var user in usersWithoutImage)
            {
                user.Image = "https://res.cloudinary.com/dpzhs3vyi/image/upload/v1732408233/default-image_zcgh1j.png";
            }

            await _userDbContext.SaveChangesAsync(); // Guardar los cambios
        }

        // Actualizar un usuario con la URL de la imagen
        public async Task<User> UpdateUserImageAsync(string userId, string imageUrl)
        {
            var user = await _userDbContext.Users.FindAsync(userId);

            if (user == null)
            {
                Console.WriteLine($"User with ID {userId} not found.");
                throw new Exception("User not found");
            }

            user.Image = imageUrl;

            try
            {
                await _userDbContext.SaveChangesAsync();
                Console.WriteLine($"User {userId} image updated successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user image: {ex.Message}");
                throw;
            }

            return user;
        }

        public async Task<List<User>> GetAssignedUsersAsync(int requirementId)
        {
            var assignedUsers = await _userDbContext.UserRequirements
                .Where(ur => ur.RequirementId == requirementId)
                .Select(ur => ur.User)
                .ToListAsync();

            return assignedUsers;
        }
    }

}