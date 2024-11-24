using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
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
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;

        public AuthService(
            IUserRepository userRepository,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _userManager = userManager;
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
                    throw new EmailNotConfirmedException();

                // Verificar si el usuario está habilitado
                if (!user.IsEnabled)
                    throw new UserNotEnabledException();

                // Validar las credenciales del usuario
                var result = await _signInManager.PasswordSignInAsync(user, loginUserDto.Password, false, false);
                if (!result.Succeeded)
                    throw new InvalidLoginException();

                // Generar el JWT y devolverlo
                return GenerateJwtToken(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during login: {ex.Message}");
                throw;
            }
        }

        public string GenerateJwtToken(User user)
        {
            if (string.IsNullOrEmpty(user.Id) || string.IsNullOrEmpty(user.UserName) || string.IsNullOrEmpty(user.Email))
                throw new IncompleteUserInfoException();

            // Obtener roles del usuario
            var roles = _userManager.GetRolesAsync(user).Result; // Uso sincrónico
            if (!roles.Any())
                throw new Exception("User has no roles assigned.");

            // Crear las claims básicas
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email)
            };

            // Agregar los roles como claims
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            // Obtener la clave secreta para el token
            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
                throw new JwtKeyNotConfiguredException();

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Crear el token
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
