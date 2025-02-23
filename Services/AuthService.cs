using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using trackit.server.Dtos;
using trackit.server.Exceptions;
using trackit.server.Models;
using trackit.server.Repositories.Interfaces;
using trackit.server.Services.Interfaces;

namespace trackit.server.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;

        public AuthService(
            IUserRepository userRepository,
            UserManager<User> userManager,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<string> LoginUserAsync(LoginUserDto loginUserDto)
        {
            // 1) Buscar el usuario
            var user = await _userRepository.GetUserByEmailAsync(loginUserDto.Email);
            if (user == null)
                throw new UserNotFoundException();

            // 2) Verificar si el correo está confirmado
            if (!user.EmailConfirmed)
                throw new EmailNotConfirmedException();

            // 3) Verificar si el usuario está habilitado
            if (!user.IsEnabled)
                throw new UserNotEnabledException();

            // 4) Validar la contraseña SIN SignInManager
            bool isPasswordValid = await _userManager.CheckPasswordAsync(user, loginUserDto.Password);
            if (!isPasswordValid)
                throw new InvalidLoginException("Invalid credentials.");

            // 5) Generar el JWT y devolverlo
            return GenerateJwtToken(user);
        }

        // Generar el JWT
        public string GenerateJwtToken(User user)
        {
            if (string.IsNullOrEmpty(user.Id) ||
                string.IsNullOrEmpty(user.UserName) ||
                string.IsNullOrEmpty(user.Email))
            {
                throw new IncompleteUserInfoException();
            }

            // Obtener roles del usuario
            var roles = _userManager.GetRolesAsync(user).Result;
            if (!roles.Any())
                throw new Exception("User has no roles assigned.");

            // Crear las claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email)
            };

            // Agregar los roles como claims
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            // Leer llave y parámetros de appsettings.json
            var jwtKey = _configuration["Jwt:Key"]
                ?? throw new JwtKeyNotConfiguredException();

            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Crear el token
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}