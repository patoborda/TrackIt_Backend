using trackit.server.Dtos;

namespace trackit.server.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetAllAsync();
        Task<CategoryDto?> GetByIdAsync(int id);
        Task<CategoryDto> AddAsync(CategoryDto dto);
        Task<CategoryDto> UpdateAsync(int id, string name, int requirementTypeId);
        Task<bool> DeleteAsync(int id);

        // Nuevo método
        Task<IEnumerable<CategoryDto>> GetByRequirementTypeAsync(int requirementTypeId);
    }
}