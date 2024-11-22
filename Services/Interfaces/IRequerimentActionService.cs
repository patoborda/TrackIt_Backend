using trackit.server.Models;

namespace trackit.server.Services.Interfaces
{
    public interface IRequirementActionService
    {
        Task LogActionAsync(int requirementId, string action, string userId, string details = null);
        Task<IEnumerable<RequirementActionLog>> GetLogsAsync(int requirementId);
    }
}
