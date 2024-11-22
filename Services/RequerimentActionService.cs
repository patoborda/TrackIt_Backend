using trackit.server.Models;
using trackit.server.Repositories.Interfaces;
using trackit.server.Services.Interfaces;

namespace trackit.server.Services
{
    public class RequirementActionService : IRequirementActionService
    {
        private readonly IRequirementActionLogRepository _logRepository;

        public RequirementActionService(IRequirementActionLogRepository logRepository)
        {
            _logRepository = logRepository;
        }

        public async Task LogActionAsync(int requirementId, string action, string userId, string details = null)
        {
            var log = new RequirementActionLog
            {
                RequirementId = requirementId,
                Action = action,
                PerformedBy = userId,
                Timestamp = DateTime.UtcNow,
                Details = details
            };

            await _logRepository.AddLogAsync(log);
        }

        public async Task<IEnumerable<RequirementActionLog>> GetLogsAsync(int requirementId)
        {
            return await _logRepository.GetLogsByRequirementIdAsync(requirementId);
        }
    }
}
