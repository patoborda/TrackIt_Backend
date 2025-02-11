using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using trackit.server.Data; // Ajusta el namespace según tu estructura

[ApiController]
[Route("api/attachments")]
public class AttachmentsController : ControllerBase
{
    private readonly UserDbContext _context;
    private readonly string _storagePath = "wwwroot/uploads";

    public AttachmentsController(UserDbContext context)
    {
        _context = context;
        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
        }
    }

    [HttpPost("{requirementId}")]
    public async Task<IActionResult> UploadFile(int requirementId, IFormFile file)
    {
        var attachmentCount = await _context.Attachments
            .Where(a => a.RequirementId == requirementId)
            .CountAsync();

        if (attachmentCount >= 5)
        {
            return BadRequest("No se pueden agregar más de 5 archivos.");
        }

        if (file == null || file.Length == 0)
        {
            return BadRequest("Archivo no válido.");
        }

        string fileName = $"{Guid.NewGuid()}_{file.FileName}";
        string filePath = Path.Combine(_storagePath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var attachment = new Attachment
        {
            RequirementId = requirementId,
            FileName = file.FileName,
            FilePath = filePath
        };

        _context.Attachments.Add(attachment);
        await _context.SaveChangesAsync();

        return Ok(attachment);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFile(int id)
    {
        var attachment = await _context.Attachments.FindAsync(id);
        if (attachment == null)
        {
            return NotFound("Archivo no encontrado.");
        }

        if (System.IO.File.Exists(attachment.FilePath))
        {
            System.IO.File.Delete(attachment.FilePath);
        }

        _context.Attachments.Remove(attachment);
        await _context.SaveChangesAsync();

        return Ok("Archivo eliminado.");
    }

    [HttpGet("{requirementId}")]
    public async Task<IActionResult> GetAttachments(int requirementId)
    {
        var attachments = await _context.Attachments
            .Where(a => a.RequirementId == requirementId)
            .Select(a => new { a.Id, a.FileName, a.FilePath })
            .ToListAsync();

        return Ok(attachments);
    }
}
