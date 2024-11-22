using trackit.server.Models;

namespace trackit.server.Repositories.Interfaces
{
    public interface IRequirementActionLogRepository
    {
        Task AddLogAsync(RequirementActionLog log);
        Task<IEnumerable<RequirementActionLog>> GetLogsByRequirementIdAsync(int requirementId);
    }
}
