using trackit.server.Dtos;

namespace trackit.server.Services.Interfaces
{
    public interface IRequirementService
    {
        Task<RequirementResponseDto> CreateRequirementAsync(RequirementCreateDto requirementDto); // Ya existente
        Task<bool> ValidateTypeAndCategoryAsync(int typeId, int categoryId); // Método para validar tipo y categoría
    }
}
