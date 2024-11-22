using trackit.server.Models;
using Microsoft.AspNetCore.Identity;
namespace trackit.server.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetUserByEmailAsync(string email);
        Task<bool> CreateUserAsync(User user, string password);
        Task<bool> UpdateUserAsync(User user);
        Task<IList<string>> GetUserRolesAsync(User user);
    }
}
