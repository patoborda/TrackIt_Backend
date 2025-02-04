using Microsoft.AspNetCore.Mvc;
using trackit.server.Dtos;
using trackit.server.Services.Interfaces;

namespace trackit.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _service;

        public CategoryController(ICategoryService service)
        {
            _service = service;
        }

        // GET api/Category
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _service.GetAllAsync();
            return Ok(categories);
        }

        // GET api/Category/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _service.GetByIdAsync(id);
            if (category == null) return NotFound();
            return Ok(category);
        }

        // POST api/Category
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CategoryDto dto)
        {
            var createdCategory = await _service.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdCategory.Id }, createdCategory);
        }

        // PUT api/Category/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CategoryUpdateDto dto)
        {
            try
            {
                var updatedCategory = await _service.UpdateAsync(id, dto.Name, dto.RequirementTypeId);
                return Ok(updatedCategory);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // DELETE api/Category/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.DeleteAsync(id);
            if (success)
            {
                return Ok(new { message = $"Category with ID {id} was successfully deleted." });
            }
            else
            {
                return NotFound(new { message = $"Category with ID {id} was not found." });
            }
        }

        // NUEVO ENDPOINT: Obtiene categorías por RequirementTypeId
        // GET api/Category/by-requirement-type/3
        [HttpGet("by-requirement-type/{requirementTypeId}")]
        public async Task<IActionResult> GetByRequirementType(int requirementTypeId)
        {
            var categories = await _service.GetByRequirementTypeAsync(requirementTypeId);
            // Puedes validar si la lista está vacía y devolver NotFound, en caso necesario
            return Ok(categories);
        }
    }
}