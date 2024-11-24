using trackit.server.Repositories.Interfaces;
using trackit.server.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

public class RequirementActionService : IRequirementActionService
{
    private readonly IRequirementActionLogRepository _logRepository;

    public RequirementActionService(IRequirementActionLogRepository logRepository)
    {
        _logRepository = logRepository;
    }

    public async Task LogActionAsync(int requirementId, string action, string details, string performedByUserId)
    {
        var log = new RequirementActionLog
        {
            RequirementId = requirementId,
            Action = action,
            Details = details,
            Timestamp = DateTime.UtcNow,
            PerformedByUserId = performedByUserId
        };

        await _logRepository.AddActionLogAsync(log);
    }

    public async Task<List<RequirementActionLog>> GetLogsByRequirementIdAsync(int requirementId)
    {
        return await _logRepository.GetLogsByRequirementIdAsync(requirementId);
    }

    public async Task<List<RequirementActionLog>> GetLogsAsync(int requirementId)
    {
        // Este método puede ser redundante con `GetLogsByRequirementIdAsync`.
        return await _logRepository.GetLogsByRequirementIdAsync(requirementId);
    }
}
