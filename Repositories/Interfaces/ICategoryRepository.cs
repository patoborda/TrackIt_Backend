using trackit.server.Models;

namespace trackit.server.Repositories.Interfaces
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllAsync();
        Task<Category?> GetByIdAsync(int id);
        Task<Category> AddAsync(Category category);
        Task<Category> UpdateAsync(Category category);
        Task<bool> DeleteAsync(int id);

        // Nuevo método
        Task<IEnumerable<Category>> GetByRequirementTypeAsync(int requirementTypeId);
    }
}