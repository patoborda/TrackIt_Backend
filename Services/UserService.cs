using Microsoft.AspNetCore.Identity;
using trackit.server.Dtos;
using trackit.server.Repositories.Interfaces;
using trackit.server.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System;
using trackit.server.Exceptions;
using trackit.server.Services.Interfaces;

namespace trackit.server.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager; // Añadir UserManager
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public UserService(IUserRepository userRepository, SignInManager<User> signInManager, IConfiguration configuration, UserManager<User> userManager, IEmailService emailService)
        {
            _userRepository = userRepository;
            _signInManager = signInManager;
            _configuration = configuration;
            _userManager = userManager;
            _emailService = emailService;
        }

        public async Task<bool> RegisterUserAsync(RegisterUserDto registerUserDto)
        {
            try
            {
                if (registerUserDto.Password != registerUserDto.ConfirmPassword)
                    throw new PasswordMismatchException();  // Excepción personalizada

                var user = new User
                {
                    FirstName = registerUserDto.FirstName,
                    LastName = registerUserDto.LastName,
                    Email = registerUserDto.Email,
                    UserName = registerUserDto.Email
                };

                var result = await _userRepository.CreateUserAsync(user, registerUserDto.Password);
                if (!result)
                    throw new UserCreationException();  // Excepción personalizada

                // Generar token de confirmación de correo electrónico
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                // Crear el enlace de confirmación (el enlace contendrá el token)
                var confirmLink = $"{_configuration["AppUrl"]}/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";

                // Crear el mensaje de correo
                var subject = "Email Confirmation";
                var body = $"<p>To confirm your email address, click the link below:</p><p><a href='{confirmLink}'>Confirm Email</a></p>";

                // Intentar enviar el correo utilizando el servicio de correo
                try
                {
                    await _emailService.SendEmailAsync(user.Email, subject, body);
                }
                catch (Exception)
                {
                    throw new EmailSendException("An error occurred while sending the email for confirmation.");
                }

                return true;
            }
            catch (Exception ex)
            {
                // El middleware manejará estas excepciones
                throw new Exception("An error occurred while registering the user.", ex);
            }
        }


        // Método para enviar un enlace de recuperación de contraseña
        public async Task<bool> SendPasswordResetLinkAsync(string email)
        {
            try
            {
                // Intentar obtener el usuario por email
                var user = await _userRepository.GetUserByEmailAsync(email);
                if (user == null)
                    throw new UserNotFoundException(); // Excepción personalizada

                // Generar el token de restablecimiento de contraseña
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                if (string.IsNullOrEmpty(token))
                    throw new PasswordResetException("Failed to generate reset token."); // Excepción personalizada

                // Crear el enlace de restablecimiento de contraseña
                var resetLink = $"{_configuration["AppUrl"]}/reset-password?token={token}";

                // Crear el mensaje de correo
                var subject = "Password Reset Request";
                var body = $"<p>To reset your password, click the link below:</p><p><a href='{resetLink}'>Reset Password</a></p>";

                // Intentar enviar el correo utilizando el servicio de correo
                try
                {
                    await _emailService.SendEmailAsync(email, subject, body);
                }
                catch (Exception)
                {
                    throw new EmailSendException("An error occurred while sending the email for password reset."); // Excepción personalizada
                }

                return true;
            }
            catch (Exception)
            {
                // Lanzar una excepción para cualquier otro error
                throw new Exception("An error occurred while processing the password reset request.");
            }
        }


        // Método para restablecer la contraseña
        public async Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            var user = await _userRepository.GetUserByEmailAsync(resetPasswordDto.Email);
            if (user == null)
                throw new UserNotFoundException(); // Excepción personalizada

            var resetResult = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.NewPassword);
            if (!resetResult.Succeeded)
                throw new InvalidOperationException("Password reset failed");

            return true;
        }


    }
}
