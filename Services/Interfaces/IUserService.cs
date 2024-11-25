using trackit.server.Dtos;
using trackit.server.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using static trackit.server.Dtos.RegisterUserDto;

namespace trackit.server.Services
{
    public interface IUserService
    {
        Task<bool> RegisterInternalUserAsync(RegisterInternalUserDto registerInternalUserDto);
        Task<bool> RegisterExternalUserAsync(RegisterExternalUserDto registerExternalUserDto);
        Task<bool> SendPasswordResetLinkAsync(string email, string clientUri);
        Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
        Task<UserProfileDto> GetUserProfileAsync(string userId);
        Task<List<UserProfileDto>> GetAllUsersAsync();
        Task<List<InternalUserProfileDto>> GetInternalUsersAsync();
        Task<List<ExternalUserProfileDto>> GetExternalUsersAsync();
        Task<bool> UpdateUserStatusAsync(string userId, bool isEnabled);
        Task<User> UploadImageAsync(IFormFile file, string userId);
        Task AssignDefaultImageToAllUsersAsync();
        Task<User> GetUserByEmailAsync(string email);
        Task SendAccountActivationEmailAsync(string email);
    }
}