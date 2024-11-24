using Microsoft.AspNetCore.Mvc;
using trackit.server.Dtos;
using trackit.server.Services.Interfaces;

namespace trackit.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RequirementTypeController : ControllerBase
    {
        private readonly IRequirementTypeService _service;

        public RequirementTypeController(IRequirementTypeService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var types = await _service.GetAllAsync();
            return Ok(types);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var type = await _service.GetByIdAsync(id);
            if (type == null) return NotFound();

            return Ok(type);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RequirementTypeDto dto)
        {
            var createdType = await _service.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdType.Id }, createdType);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] RequirementTypeDto dto)
        {
            try
            {
                var updatedType = await _service.UpdateAsync(id, dto);
                return Ok(updatedType);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.DeleteAsync(id);
            if (!success) return NotFound();

            return NoContent();
        }
    }

}
