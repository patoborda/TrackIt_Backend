using trackit.server.Dtos;
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
    }

}
