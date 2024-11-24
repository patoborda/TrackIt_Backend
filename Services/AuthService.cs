using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using trackit.server.Dtos;
using trackit.server.Models;
using trackit.server.Exceptions;
using Microsoft.IdentityModel.Tokens;
using trackit.server.Services.Interfaces;
using trackit.server.Repositories.Interfaces;

namespace trackit.server.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;

        public AuthService(IUserRepository userRepository, SignInManager<User> signInManager, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        public async Task<string> LoginUserAsync(LoginUserDto loginUserDto)
        {
            try
            {
                // Verificar que el usuario existe
                var user = await _userRepository.GetUserByEmailAsync(loginUserDto.Email);
                if (user == null)
                    throw new UserNotFoundException();

                // Verificar si el correo está confirmado
                if (!user.EmailConfirmed)
                {
                    throw new EmailNotConfirmedException();
                }

                // Verificar si el usuario está habilitado
                if (!user.IsEnabled)
                {
                    throw new UserNotEnabledException();
                }

                // Validar las credenciales del usuario
                var result = await _signInManager.PasswordSignInAsync(user, loginUserDto.Password, false, false);
                if (!result.Succeeded)
                    throw new InvalidLoginException();

                // Si el login es exitoso, generar el JWT
                return GenerateJwtToken(user);
            }
            catch (UserNotFoundException ex)
            {
                throw new Exception("User not found.", ex);
            }
            catch (EmailNotConfirmedException ex)
            {
                throw new Exception("Email not confirmed. Please confirm your email before logging in.", ex);
            }
            catch (UserNotEnabledException ex)
            {
                throw new Exception("User account is not enabled. Please contact the administrator.", ex);
            }
            catch (InvalidLoginException ex)
            {
                throw new Exception("Invalid login credentials.", ex);
            }
            catch (JwtGenerationException ex)
            {
                throw new Exception("Error generating JWT token.", ex);
            }
            catch (Exception ex)
            {
                // Manejo de cualquier otra excepción inesperada
                throw new Exception("An error occurred during login.", ex);
            }
        }

        public string GenerateJwtToken(User user)
        {
            try
            {
                if (string.IsNullOrEmpty(user.Id) || string.IsNullOrEmpty(user.UserName) || string.IsNullOrEmpty(user.Email))
                {
                    throw new IncompleteUserInfoException();
                }

                // Crear las claims
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Email, user.Email)
                };

                // Obtener la clave secreta
                var jwtKey = _configuration["Jwt:Key"];
                if (string.IsNullOrEmpty(jwtKey))
                {
                    throw new JwtKeyNotConfiguredException();
                }

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    _configuration["Jwt:Issuer"],
                    _configuration["Jwt:Audience"],
                    claims,
                    expires: DateTime.Now.AddHours(2),
                    signingCredentials: creds
                );

                // Si el token se genera correctamente, loguea el evento (para depuración)
                Console.WriteLine("JWT Token successfully generated.");
                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                // Registra el error específico
                Console.WriteLine($"Error generating JWT: {ex.Message}");
                throw new JwtGenerationException();
            }
        }
    }
}