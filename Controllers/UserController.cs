using Microsoft.AspNetCore.Mvc;
using trackit.server.Dtos;
using trackit.server.Services;
using trackit.server.Exceptions;
using System.Threading.Tasks;
using trackit.server.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using trackit.server.Models;
using System.Security.Claims;

namespace trackit.server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IAuthService _authService;
        private readonly UserManager<User> _userManager;
        private readonly IEmailService _emailService;
        public UserController(IUserService userService, IAuthService authService, UserManager<User> userManager, IEmailService emailService)
        {
            _userService = userService;
            _authService = authService;
            _userManager = userManager;
            _emailService = emailService;
        }

        // Endpoint para registrar un nuevo usuario
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto registerUserDto)
        {
            try
            {
                var result = await _userService.RegisterUserAsync(registerUserDto);
                if (result)
                {
                    return Ok(new { message = "User registered successfully!" });
                }
                return BadRequest(new { message = "Failed to register user." });
            }
            catch (UserCreationException)
            {
                return BadRequest(new { message = "User could not be created." });
            }
            catch (PasswordMismatchException)
            {
                return BadRequest(new { message = "Passwords do not match." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // Endpoint para iniciar sesión (login) de un usuario
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDto loginUserDto)
        {
            try
            {
                var token = await _authService.LoginUserAsync(loginUserDto);
                return Ok(new { token });
            }
            catch (UserNotFoundException)
            {
                return NotFound(new { message = "User not found." });
            }
            catch (InvalidLoginException)
            {
                return Unauthorized(new { message = "Invalid login credentials." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // Acción para confirmar el correo electrónico
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return BadRequest("Invalid email confirmation request.");
            }

            // Decodificar el token (importante si está codificado en el enlace)
            token = Uri.UnescapeDataString(token);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return BadRequest("User not found.");
            }

            Console.WriteLine($"User found: {user.Email}");
            Console.WriteLine($"Email confirmed: {user.EmailConfirmed}");

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                Console.WriteLine("Email confirmed successfully.");
                return Ok("Email confirmed successfully.");
            }
            else
            {
                Console.WriteLine($"Error confirming email: {result.Errors.FirstOrDefault()?.Description}");
                return BadRequest("Error confirming email.");
            }
        }



        // Endpoint para enviar el enlace de recuperación de contraseña
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            try
            {
                var result = await _userService.SendPasswordResetLinkAsync(forgotPasswordDto.Email);
                return Ok(new { message = "Password reset link sent successfully" });
            }
            catch (UserNotFoundException)
            {
                return NotFound(new { message = "User not found" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while sending the password reset link" });
            }
        }


        // Endpoint para restablecer la contraseña
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            if (resetPasswordDto.NewPassword != resetPasswordDto.ConfirmPassword)
                return BadRequest(new { message = "Passwords do not match" });

            try
            {
                var result = await _userService.ResetPasswordAsync(resetPasswordDto);
                return Ok(new { message = "Password reset successfully" });
            }
            catch (UserNotFoundException)
            {
                return NotFound(new { message = "User not found" });
            }
            catch (InvalidOperationException)
            {
                return BadRequest(new { message = "Invalid token or password reset failed" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while resetting the password" });
            }
        }

        // Endpoint para obtener el perfil del usuario autenticado
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                // Obtener el ID del usuario de las claims del JWT
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User ID not found.");
                }

                // Obtener el usuario desde la base de datos usando el UserManager
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    return NotFound("User not found.");
                }

                // Verificar si alguna propiedad importante del usuario es null
                if (string.IsNullOrEmpty(user.FirstName) || string.IsNullOrEmpty(user.LastName) || string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.UserName))
                {
                    throw new UserProfileException("User profile contains null or empty values for critical properties.");
                }

                // Mapear la información del usuario al DTO
                var userProfile = new UserProfileDto
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    UserName = user.UserName
                };

                return Ok(userProfile);
            }
            catch (UserProfileException ex)
            {
                return StatusCode(400, new { message = ex.Message }); // Código 400 Bad Request
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message }); // Código 500 Internal Server Error
            }
        }




        [HttpGet("test-email")]
        public async Task<IActionResult> TestEmail()
        {
            string to = "trackit900@gmail.com";  // Correo de prueba
            string subject = "Test Email";
            string message = "This is a test email.";

            try
            {
                await _emailService.SendEmailAsync(to, subject, message);
                return Ok("Email sent successfully.");
            }
            catch (EmailSendException ex)
            {
                return StatusCode(500, $"Error sending email: {ex.Message}");
            }
        }

    }
}
