﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using trackit.server.Dtos;
using trackit.server.Services.Interfaces;

namespace trackit.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requiere autenticación para todos los endpoints por defecto
    public class RequirementsController : ControllerBase
    {
        private readonly IRequirementService _requirementService;
        private readonly IRequirementActionService _actionService;

        public RequirementsController(IRequirementService requirementService, IRequirementActionService actionService)
        {
            _requirementService = requirementService;
            _actionService = actionService;
        }

        // Crear un requerimiento
        [HttpPost]
        public async Task<IActionResult> CreateRequirement([FromBody] RequirementCreateDto requirementDto)
        {
            if (requirementDto == null)
                return BadRequest(new { Message = "Invalid request payload." });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { Message = "User is not authenticated." });

            try
            {
                var response = await _requirementService.CreateRequirementAsync(requirementDto, userId);
                return CreatedAtAction(nameof(GetRequirementById), new { id = response.Id }, new
                {
                    Message = "Requirement created successfully",
                    Data = response
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while creating the requirement.", Error = ex.Message });
            }
        }

        // Actualizar un requerimiento
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRequirement(int id, [FromBody] RequirementUpdateDto updateDto)
        {
            if (updateDto == null)
                return BadRequest(new { Message = "Invalid request payload." });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { Message = "User is not authenticated." });

            try
            {
                var response = await _requirementService.UpdateRequirementAsync(id, updateDto, userId);
                return Ok(new
                {
                    Message = "Requirement updated successfully",
                    Data = response
                });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while updating the requirement.", Error = ex.Message });
            }
        }

        // Obtener un requerimiento por ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRequirementById(int id)
        {
            try
            {
                var requirement = await _requirementService.GetRequirementByIdAsync(id);
                return Ok(new
                {
                    Message = "Requirement retrieved successfully",
                    Data = requirement
                });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving the requirement.", Error = ex.Message });
            }
        }

        // Obtener todos los requerimientos (Solo Admin)
        [Authorize(Roles = "Admin, Interno")] // Solo accesible para usuarios con el rol Admin
        [HttpGet]
        public async Task<IActionResult> GetAllRequirements()
        {
            try
            {
                var requirements = await _requirementService.GetAllRequirementsAsync();
                return Ok(new
                {
                    Message = "Requirements retrieved successfully",
                    Data = requirements
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving the requirements.", Error = ex.Message });
            }
        }

        // Eliminar un requerimiento
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRequirement(int id)
        {
            try
            {
                await _requirementService.DeleteRequirementAsync(id);
                return Ok(new { Message = "Requirement deleted successfully" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while deleting the requirement.", Error = ex.Message });
            }
        }

        // Obtener los logs de acciones de un requerimiento
        [HttpGet("{requirementId}/logs")]
        public async Task<IActionResult> GetRequirementLogs(int requirementId)
        {
            try
            {
                var logs = await _actionService.GetLogsAsync(requirementId);
                return Ok(new
                {
                    Message = "Logs retrieved successfully",
                    Data = logs
                });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving the logs.", Error = ex.Message });
            }
        }

        // Endpoint público (sin autenticación)
        [AllowAnonymous] // Permite acceso sin autenticación
        [HttpGet("public-endpoint")]
        public IActionResult PublicEndpoint()
        {
            return Ok(new { Message = "This endpoint is public!" });
        }
    }
}
