using trackit.server.Data;
using trackit.server.Models;
using Microsoft.EntityFrameworkCore;
using trackit.server.Repositories.Interfaces;

namespace trackit.server.Repositories
{
    public class PriorityRepository : IPriorityRepository
    {
        private readonly UserDbContext _context;

        public PriorityRepository(UserDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Priority>> GetAllAsync()
        {
            return await _context.Priorities.ToListAsync();
        }

        public async Task<Priority?> GetByIdAsync(int id)
        {
            return await _context.Priorities.FindAsync(id);
        }

        public async Task<Priority> AddAsync(Priority priority)
        {
            _context.Priorities.Add(priority);
            await _context.SaveChangesAsync();
            return priority;
        }

        public async Task<Priority> UpdateAsync(Priority priority)
        {
            _context.Priorities.Update(priority);
            await _context.SaveChangesAsync();
            return priority;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var priority = await _context.Priorities.FindAsync(id);
            if (priority == null) return false;

            _context.Priorities.Remove(priority);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
