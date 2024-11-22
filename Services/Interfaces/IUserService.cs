using trackit.server.Dtos;


namespace trackit.server.Services
{
    public interface IUserService
    {
        Task<bool> RegisterUserAsync(RegisterUserDto registerUserDto);
        Task<bool> SendPasswordResetLinkAsync(string email);
        Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
    }

}
