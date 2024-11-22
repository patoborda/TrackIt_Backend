using System.Threading.Tasks;
using trackit.server.Dtos;
using trackit.server.Models;

namespace trackit.server.Services.Interfaces
{
    public interface IAuthService
    {
        Task<string> LoginUserAsync(LoginUserDto loginUserDto);
        string GenerateJwtToken(User user);
    }
}
