using Microsoft.AspNetCore.Mvc;
using trackit.server.Dtos;
using trackit.server.Services.Interfaces;

namespace trackit.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PriorityController : ControllerBase
    {
        private readonly IPriorityService _service;

        public PriorityController(IPriorityService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var priorities = await _service.GetAllAsync();
            return Ok(priorities);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var priority = await _service.GetByIdAsync(id);
            if (priority == null) return NotFound(new { message = $"Priority with ID {id} not found." });

            return Ok(priority);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PriorityDto dto)
        {
            var createdPriority = await _service.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdPriority.Id }, createdPriority);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PriorityUpdateDto dto)
        {
            try
            {
                var updatedPriority = await _service.UpdateAsync(id, dto.TypePriority);
                return Ok(updatedPriority);
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
            if (success)
            {
                return Ok(new { message = $"Priority with ID {id} was successfully deleted." });
            }
            else
            {
                return NotFound(new { message = $"Priority with ID {id} not found." });
            }
        }
    }
}
