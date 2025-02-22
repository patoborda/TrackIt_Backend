using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Threading.Tasks;
using trackit.server.Data; // Ajusta el namespace según tu estructura
using trackit.server.Hubs;


[ApiController]
[Route("api/comments")]
public class CommentsController : ControllerBase
{
    private readonly UserDbContext _context;
    private readonly IHubContext<CommentHub> _hubContext;

    public CommentsController(UserDbContext context, IHubContext<CommentHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }
    [HttpPost]
    public async Task<IActionResult> PostComment(Comment comment)
    {
        // Obtener cuántos archivos hay en el requerimiento
        int totalFiles = _context.Comments
            .Where(c => c.RequirementId == comment.RequirementId)
            .SelectMany(c => c.Attachments) // ✅ Usamos Attachments en lugar de Files
            .Count();

        if (totalFiles + comment.Attachments.Count > 5)
            return BadRequest(new { Message = "El requerimiento ya tiene 5 archivos adjuntos." });

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        // Enviar el comentario en tiempo real a los usuarios conectados al mismo requerimiento
        await _hubContext.Clients.Group(comment.RequirementId.ToString())
            .SendAsync("ReceiveComment", comment.UserName, comment.Description);

        return Ok(comment);
    }


    [HttpGet("{requirementId}")]
    public IActionResult GetComments(int requirementId)
    {
        var comments = _context.Comments
            .Where(c => c.RequirementId == requirementId)
            .OrderBy(c => c.CreatedAt)
            .ToList();

        return Ok(comments);
    }
}
