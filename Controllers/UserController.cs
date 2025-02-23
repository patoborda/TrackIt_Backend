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
using Microsoft.AspNetCore.Authorization;

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
        private readonly IConfiguration _configuration;  // Agregar configuración

        public UserController(IUserService userService, IAuthService authService, UserManager<User> userManager, IEmailService emailService, IConfiguration configuration)
        {
            _userService = userService;
            _authService = authService;
            _userManager = userManager;
            _emailService = emailService;
            _configuration = configuration;
        }

        // Endpoint para registrar un nuevo usuario interno
        [HttpPost("register-internal")]
        public async Task<IActionResult> RegisterInternal([FromBody] RegisterInternalUserDto registerInternalUserDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _userService.RegisterInternalUserAsync(registerInternalUserDto);
                if (result)
                {
                    // El UserService ya envía el correo de confirmación internamente
                    return Ok(new { message = "User registered successfully! Please check your email to confirm your account." });
                }
                return BadRequest(new { message = "Failed to register user." });
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

        // Endpoint para registrar un nuevo usuario externo
        [HttpPost("register-external")]
        public async Task<IActionResult> RegisterExternal([FromBody] RegisterExternalUserDto registerExternalUserDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _userService.RegisterExternalUserAsync(registerExternalUserDto);
                if (result)
                {
                    // El UserService ya envía el correo de confirmación internamente
                    return Ok(new { message = "User registered successfully! Please check your email to confirm your account." });
                }
                return BadRequest(new { message = "Failed to register user." });
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
            catch (Exception ex)
            {
                // Manejar excepciones específicas basadas en el mensaje
                if (ex.InnerException is EmailNotConfirmedException)
                {
                    return Unauthorized(new { message = "Please confirm your email before logging in." });
                }
                else if (ex.InnerException is UserNotEnabledException)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new { message = "Your account is not enabled. Please contact the administrator." });
                }
                else if (ex.InnerException is UserNotFoundException)
                {
                    return NotFound(new { message = "User not found." });
                }
                else if (ex.InnerException is InvalidLoginException)
                {
                    return Unauthorized(new { message = "Invalid login credentials." });
                }
                else
                {
                    return StatusCode(500, new { message = ex.Message });
                }
            }
        }

        // Endpoint para enviar el enlace de recuperación de contraseña
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // 1) Delegamos todo al servicio
                await _userService.ForgotPasswordAsync(forgotPasswordDto.Email, forgotPasswordDto.ClientUri);

                // 2) Retornamos un mensaje de éxito
                return Ok(new { message = "Password reset link sent successfully" });
            }
            catch (UserNotFoundException)
            {
                return NotFound(new { message = "User not found" });
            }
            catch (Exception ex)
            {
                // Puedes refinar el manejo de excepciones si quieres
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // Endpoint para restablecer la contraseña
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _userService.ResetPasswordAsync(resetPasswordDto);
                return Ok(new { message = "Password reset successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UserNotFoundException)
            {
                return NotFound(new { message = "User not found." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while resetting the password", details = ex.Message });
            }
        }

        // Endpoint para obtener el perfil del usuario
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

            // Descodificar el token (si está codificado con %2B, %2F, etc.)
            token = Uri.UnescapeDataString(token);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return BadRequest("User not found.");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                // Aquí simplemente retornamos Ok SIN enviar correo adicional
                return Ok("Email confirmed successfully.");
            }
            else
            {
                // Para ver el detalle del error Identity
                var errorDetails = string.Join("; ", result.Errors.Select(e => e.Description));
                return BadRequest($"Error confirming email. Details: {errorDetails}");
            }
        }

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

        // Endpoint para obtener un usuario por su ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            try
            {
                var user = await _userService.GetUserProfileAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "User not found." });
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}