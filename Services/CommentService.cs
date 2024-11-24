using trackit.server.Models;
using trackit.server.Repositories;
using trackit.server.Services;
using trackit.server.Dtos;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _repository;

    public CommentService(ICommentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Comment> CreateCommentAsync(CreateCommentDto commentDto)
    {
        var comment = new Comment
        {
            Subject = commentDto.Subject,
            Description = commentDto.Description,
            Date = DateTime.UtcNow,
            Time = DateTime.UtcNow.TimeOfDay,
            RequirementId = commentDto.RequirementId,  // Mantén como int
            UserId = commentDto.UserId
        };

        if (commentDto.Files != null)
        {
            foreach (var file in commentDto.Files)
            {
                comment.Files.Add(new AttachedFile { Link = file });
            }
        }

        return await _repository.CreateCommentAsync(comment);
    }


    public async Task<Comment> GetCommentByIdAsync(int id)
    {
        return await _repository.GetCommentByIdAsync(id);
    }

    // Asegúrate de que 'requirementId' sea de tipo int, no de tipo string
    public async Task<IEnumerable<Comment>> GetCommentsByRequirementAsync(int requirementId)
    {
        return await _repository.GetCommentsByRequirementAsync(requirementId);  // Pasa el 'int' directamente
    }

    public async Task UpdateCommentAsync(int id, CreateCommentDto commentDto)
    {
        var existingComment = await _repository.GetCommentByIdAsync(id);
        if (existingComment == null) throw new KeyNotFoundException();

        existingComment.Subject = commentDto.Subject;
        existingComment.Description = commentDto.Description;
        existingComment.Date = DateTime.UtcNow;
        existingComment.Time = DateTime.UtcNow.TimeOfDay;

        await _repository.UpdateCommentAsync(existingComment);
    }

    public async Task DeleteCommentAsync(int id)
    {
        await _repository.DeleteCommentAsync(id);
    }
}
