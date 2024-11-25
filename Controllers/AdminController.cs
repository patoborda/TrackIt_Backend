using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using trackit.server.Services;
using System.Linq;
using System.Threading.Tasks;
using trackit.server.Dtos;

namespace trackit.server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]  // Solo permitir acceso a usuarios autenticados
    public class AdminController : ControllerBase
    {
        private readonly IUserService _userService;

        public AdminController(IUserService userService)
        {
            _userService = userService;
        }

        // Endpoint para obtener todos los usuarios (Solo Admin)
        [HttpGet("GetAllUsers")]
        [Authorize(Roles = "Admin")]  // Solo Admin puede acceder a este endpoint
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                if (users == null || !users.Any())
                {
                    return NotFound("No users found.");
                }
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // Endpoint para obtener usuarios externos (Solo Admin)
        [HttpGet("GetExternalUsers")]
        [Authorize(Roles = "Admin")]  // Solo Admin puede acceder a este endpoint
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

        // Endpoint para obtener usuarios internos (Admin e Interno pueden acceder)
        [HttpGet("GetInternalUsers")]
        [Authorize(Roles = "Admin,Internal")]  // Admin e Interno pueden acceder
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

        // Endpoint para asignar la imagen por defecto a todos los usuarios (Solo Admin)
        [HttpPost("assign-default-image")]
        [Authorize(Roles = "Admin")]  // Solo Admin puede acceder a este endpoint
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

        [HttpPut("UpdateUserStatus")]
        [Authorize(Roles = "Admin")]  // Solo Admin puede acceder a este endpoint
        public async Task<IActionResult> UpdateUserStatus([FromBody] UpdateUserStatusDto updateDto)
        {
            try
            {
                // Obtener el usuario por correo electrónico
                var user = await _userService.GetUserByEmailAsync(updateDto.Email);

                if (user == null)
                {
                    return NotFound("User not found with the provided email.");
                }

                // Cambiar el estado del usuario
                user.IsEnabled = updateDto.IsEnabled;

                // Actualizar el usuario
                var result = await _userService.UpdateUserStatusAsync(user.Id, user.IsEnabled);
                if (result)
                {
                    return Ok("User status updated successfully.");
                }

                return StatusCode(500, "Error updating user status.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }





    }
}
