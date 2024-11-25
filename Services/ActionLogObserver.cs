using trackit.server.Patterns.Observer;
using trackit.server.Repositories.Interfaces;

public class ActionLogObserver : IObserver
{
    private readonly IRequirementActionLogRepository _actionLogRepository;

    public ActionLogObserver(IRequirementActionLogRepository actionLogRepository)
    {
        _actionLogRepository = actionLogRepository;
    }

    public async Task NotifyAsync(string message, object data)
    {
        var logEntry = new RequirementActionLog
        {
            RequirementId = ((Requirement)data).Id, // Cast data a Requirement
            Action = message,
            PerformedByUserId = "system", // Puedes parametrizar esto si es necesario
            Details = "Action logged",
            Timestamp = DateTime.UtcNow
        };

        await _actionLogRepository.AddActionLogAsync(logEntry);
    }
}
