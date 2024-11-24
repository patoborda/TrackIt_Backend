using trackit.server.Models;

namespace trackit.server.Repositories.Interfaces
{
    public interface IPriorityRepository
    {
        Task<IEnumerable<Priority>> GetAllAsync();
        Task<Priority?> GetByIdAsync(int id);
        Task<Priority> AddAsync(Priority priority);
        Task<Priority> UpdateAsync(Priority priority);
        Task<bool> DeleteAsync(int id);
    }
}
