using Microsoft.AspNetCore.Mvc;
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

        public RequirementsController(IRequirementService service)
        {
            _service = service;
        }

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

    }

}
