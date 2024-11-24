using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using trackit.server.Dtos;
using trackit.server.Services;

namespace trackit.server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _service;

        public CommentController(ICommentService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> CreateComment([FromBody] CreateCommentDto commentDto)
        {
            var comment = await _service.CreateCommentAsync(commentDto);
            return CreatedAtAction(nameof(GetCommentById), new { id = comment.Id }, comment);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCommentById(int id)
        {
            var comment = await _service.GetCommentByIdAsync(id);
            if (comment == null) return NotFound();
            return Ok(comment);
        }

        [HttpGet("requirement/{requirementId}")]
        public async Task<IActionResult> GetCommentsByRequirement(int requirementId)
        {
            var comments = await _service.GetCommentsByRequirementAsync(requirementId);
            return Ok(comments);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateComment(int id, [FromBody] CreateCommentDto commentDto)
        {
            await _service.UpdateCommentAsync(id, commentDto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            await _service.DeleteCommentAsync(id);
            return NoContent();
        }
    }
}
