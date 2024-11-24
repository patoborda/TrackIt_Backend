using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using trackit.server.Dtos;
using trackit.server.Services.Interfaces;

namespace trackit.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; // Obtener el ID del usuario autenticado

            // Validar tipo y categoría
            var isValid = await _requirementService.ValidateTypeAndCategoryAsync(requirementDto.RequirementTypeId, requirementDto.CategoryId);
            if (!isValid)
            {
                return BadRequest(new { Message = "The category does not belong to the specified type." });
            }

            // Crear el requerimiento
            var response = await _requirementService.CreateRequirementAsync(requirementDto, userId);

            return Ok(new
            {
                Message = "Requirement created successfully",
                Data = response
            });
        }

        // Actualizar un requerimiento
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRequirement(int id, [FromBody] RequirementUpdateDto updateDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; // Obtener el ID del usuario autenticado

            // Actualizar el requerimiento
            var response = await _requirementService.UpdateRequirementAsync(id, updateDto, userId);

            return Ok(new
            {
                Message = "Requirement updated successfully",
                Data = response
            });
        }

        // Obtener un requerimiento por ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRequirementById(int id)
        {
            var requirement = await _requirementService.GetRequirementByIdAsync(id);

            return Ok(new
            {
                Message = "Requirement retrieved successfully",
                Data = requirement
            });
        }

        // Obtener todos los requerimientos
        [HttpGet]
        public async Task<IActionResult> GetAllRequirements()
        {
            var requirements = await _requirementService.GetAllRequirementsAsync();

            return Ok(new
            {
                Message = "Requirements retrieved successfully",
                Data = requirements
            });
        }

        // Eliminar un requerimiento
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRequirement(int id)
        {
            await _requirementService.DeleteRequirementAsync(id);

            return Ok(new { Message = "Requirement deleted successfully" });
        }

        // Obtener los logs de acciones de un requerimiento
        [HttpGet("{requirementId}/logs")]
        public async Task<IActionResult> GetRequirementLogs(int requirementId)
        {
            var logs = await _actionService.GetLogsAsync(requirementId);

            return Ok(new
            {
                Message = "Logs retrieved successfully",
                Data = logs
            });
        }
    }
}
