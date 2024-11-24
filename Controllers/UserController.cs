using Microsoft.AspNetCore.Mvc;
using trackit.server.Dtos;
using trackit.server.Services;
using trackit.server.Exceptions;
using System.Threading.Tasks;
using trackit.server.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using trackit.server.Models;
using System.Security.Claims;
using static trackit.server.Dtos.RegisterUserDto;
using Microsoft.EntityFrameworkCore;

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
        [HttpPost("register-internal")]
        public async Task<IActionResult> RegisterInternal([FromBody] RegisterInternalUserDto registerInternalUserDto)
        {
            try
            {
                var result = await _userService.RegisterInternalUserAsync(registerInternalUserDto);
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


        // Endpoint para registrar un nuevo usuario
        [HttpPost("register-external")]
        public async Task<IActionResult> RegisterExternal([FromBody] RegisterExternalUserDto registerExternalUserDto)
        {
            try
            {
                var result = await _userService.RegisterExternalUserAsync(registerExternalUserDto);
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

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User ID not found.");
                }

                var userProfile = await _userService.GetUserProfileAsync(userId);
                return Ok(userProfile);
            }
            catch (UserNotFoundException)
            {
                return NotFound("User not found.");
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

        [HttpGet("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                // Obtener todos los usuarios excluyendo administradores
                var users = await _userService.GetAllUsersAsync();  // Obtener todos los usuarios y mapear a DTO

                if (users == null || !users.Any())
                {
                    return NotFound("No users found.");
                }

                return Ok(users);  // Devolver los usuarios con sus respectivos DTOs
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // Endpoint para obtener los usuarios externos habilitados
        [HttpGet("GetExternalUsers")]
        public async Task<IActionResult> GetExternalUsers()
        {
            try
            {
                var externalUsers = await _userService.GetExternalUsersAsync();

                if (externalUsers == null || !externalUsers.Any())
                {
                    return NotFound("No external users found.");
                }

                return Ok(externalUsers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // Endpoint para obtener los usuarios internos habilitados
        [HttpGet("GetInternalUsers")]
        public async Task<IActionResult> GetInternalUsers()
        {
            try
            {
                var internalUsers = await _userService.GetInternalUsersAsync();

                if (internalUsers == null || !internalUsers.Any())
                {
                    return NotFound("No internal users found.");
                }

                return Ok(internalUsers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }


        /************************************************************************************************/

        // Endpoint para subir la imagen del usuario
        [HttpPost("upload-image/{userId}")]
        public async Task<IActionResult> UploadImage(string userId, IFormFile file)
        {
            try
            {
                var updatedUser = await _userService.UploadImageAsync(file, userId);
                return Ok(updatedUser); // Devuelve el usuario actualizado con la URL de la imagen
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // Endpoint para asignar imagen por defecto a todos los usuarios sin imagen
        [HttpPost("assign-default-image")]
        public async Task<IActionResult> AssignDefaultImageToAllUsers()
        {
            try
            {
                await _userService.AssignDefaultImageToAllUsersAsync();
                return Ok("Default image assigned to users without an image.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }




    }
}
