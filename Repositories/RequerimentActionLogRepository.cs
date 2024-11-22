using trackit.server.Models;
using trackit.server.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using trackit.server.Data;


namespace trackit.server.Repositories
{
    public class RequirementActionLogRepository : IRequirementActionLogRepository
    {
        private readonly UserDbContext _context;

        public RequirementActionLogRepository(UserDbContext context)
        {
            _context = context;
        }

        public async Task AddLogAsync(RequirementActionLog log)
        {
            _context.RequirementActionLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<RequirementActionLog>> GetLogsByRequirementIdAsync(int requirementId)
        {
            return await _context.RequirementActionLogs
                .Where(log => log.RequirementId == requirementId)
                .OrderByDescending(log => log.Timestamp)
                .ToListAsync();
        }
    }
}
