﻿using Microsoft.AspNetCore.Mvc;
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

        /// Endpoint para registrar un nuevo usuario interno.
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


        /// Endpoint para registrar un nuevo usuario externo.
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
                var result = await _userService.SendPasswordResetLinkAsync(forgotPasswordDto.Email, forgotPasswordDto.ClientUri);
                return Ok(new { message = "Password reset link sent successfully" });
            }
            catch (UserNotFoundException)
            {
                return NotFound(new { message = "User not found" });
            }
            catch (EmailSendException ex)
            {
                // Puedes loguear ex.Message aquí si lo deseas
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while sending the password reset link" });
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
