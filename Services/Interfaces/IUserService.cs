using trackit.server.Dtos;
using trackit.server.Models;
using static trackit.server.Dtos.RegisterUserDto;


namespace trackit.server.Services
{
    public interface IUserService
    {
        Task<bool> RegisterInternalUserAsync(RegisterInternalUserDto registerInternalUserDto);
        Task<bool> RegisterExternalUserAsync(RegisterExternalUserDto registerExternalUserDto);
        Task<bool> SendPasswordResetLinkAsync(string email);
        Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
        Task<UserProfileDto> GetUserProfileAsync(string userId);
        Task<List<UserProfileDto>> GetAllUsersAsync();
        Task<List<InternalUserProfileDto>> GetInternalUsersAsync();
        Task<List<ExternalUserProfileDto>> GetExternalUsersAsync();


        /*******************************************************************/
        Task<User> UploadImageAsync(IFormFile file, string userId);
        Task AssignDefaultImageToAllUsersAsync();
    }

}
