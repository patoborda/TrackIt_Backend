using trackit.server.Dtos;

namespace trackit.server.Services.Interfaces
{
    public interface IRequirementTypeService
    {
        Task<IEnumerable<RequirementTypeDto>> GetAllAsync();
        Task<RequirementTypeDto?> GetByIdAsync(int id);
        Task<RequirementTypeDto> AddAsync(RequirementTypeDto dto);
        Task<RequirementTypeDto> UpdateAsync(int id, string name); // Cambiado aquí
        Task<bool> DeleteAsync(int id);
    }

}
