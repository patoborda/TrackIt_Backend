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
        private readonly IRequirementService _service;
        private readonly IRequirementActionService _actionService; // Servicio para manejar los logs de acciones

        public RequirementsController(IRequirementService service, IRequirementActionService actionService)
        {
            _service = service;
            _actionService = actionService;
        }

        // Crear un requerimiento
        [HttpPost]
        public async Task<IActionResult> CreateRequirement([FromBody] RequirementCreateDto requirementDto)
        {
            var isValid = await _service.ValidateTypeAndCategoryAsync(requirementDto.RequirementTypeId, requirementDto.CategoryId);
            if (!isValid)
            {
                return BadRequest("The category does not belong to the specified type.");
            }

            var response = await _service.CreateRequirementAsync(requirementDto);
            return Ok(new
            {
                Message = "Requirement created successfully",
                Data = response
            });
        }

        // Actualizar un requerimiento (solo para usuarios internos)
        //[Authorize(Roles = "Interno")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRequirement(int id, [FromBody] RequirementUpdateDto updateDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; // Obtener el ID del usuario autenticado

            var response = await _service.UpdateRequirementAsync(id, updateDto, userId);

            return Ok(new
            {
                Message = "Requirement updated successfully",
                Data = response
            });
        }

        // Obtener el historial de acciones de un requerimiento (solo para usuarios internos)
        //[Authorize(Roles = "Interno")]
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
