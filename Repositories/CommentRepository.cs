using Microsoft.EntityFrameworkCore;
using trackit.server.Models;
using trackit.server.Data;

namespace trackit.server.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly UserDbContext _context;

        public CommentRepository(UserDbContext context)
        {
            _context = context;
        }

        public async Task<Comment> CreateCommentAsync(Comment comment)
        {
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task<Comment> GetCommentByIdAsync(int id)
        {
            return await _context.Comments.Include(c => c.Files).FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Comment>> GetCommentsByRequirementAsync(int requirementId) // Cambiar a int
        {
            return await _context.Comments
                .Where(c => c.RequirementId == requirementId) // Ahora comparas int con int
                .Include(c => c.Files)
                .ToListAsync();
        }




        public async Task UpdateCommentAsync(Comment comment)
        {
            _context.Entry(comment).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCommentAsync(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment != null)
            {
                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();
            }
        }
    }
}
