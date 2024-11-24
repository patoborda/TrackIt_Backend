using trackit.server.Repositories.Interfaces;
using trackit.server.Services;

public class ActionLogObserver 
{
    private readonly IRequirementActionLogRepository _actionLogRepository;

    public ActionLogObserver(IRequirementActionLogRepository actionLogRepository)
    {
        _actionLogRepository = actionLogRepository;
    }

    public async Task UpdateAsync(Requirement requirement, string action, string userId, string details)
    {
        var logEntry = new RequirementActionLog
        {
            RequirementId = requirement.Id,
            Action = action,
            PerformedByUserId = userId,
            Details = details,
            Timestamp = DateTime.UtcNow
        };

        await _actionLogRepository.AddActionLogAsync(logEntry);
    }
}
