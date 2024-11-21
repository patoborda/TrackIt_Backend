using trackit.server.Models;

namespace trackit.server.Repositories.Interfaces
{
    public interface IRequirementRepository
    {
        Task<Requirement> AddAsync(Requirement requirement);
        Task<int> GetNextSequentialNumberAsync();
        Task<bool> ValidateTypeAndCategoryAsync(int typeId, int categoryId);
        Task<string> GetRequirementTypeNameAsync(int typeId); // Nueva definición
        Task<string> GetCategoryNameAsync(int categoryId); // Nueva definición
    }
}
