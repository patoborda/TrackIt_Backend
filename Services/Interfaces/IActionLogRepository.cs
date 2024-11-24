namespace trackit.server.Repositories.Interfaces
{
    public interface IActionLogRepository
    {
        Task AddLogAsync(RequirementActionLog actionLog);
        Task<List<RequirementActionLog>> GetLogsByRequirementIdAsync(int requirementId);
    }
}
