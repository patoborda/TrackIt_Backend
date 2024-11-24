using trackit.server.Models;

namespace trackit.server.Services.Interfaces
{
    public interface IRequirementActionService
    {
        Task LogActionAsync(int requirementId, string action, string details, string performedByUserId);
        Task<List<RequirementActionLog>> GetLogsByRequirementIdAsync(int requirementId);
        Task<List<RequirementActionLog>> GetLogsAsync(int requirementId);

    }


}
