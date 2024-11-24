using trackit.server.Dtos;

namespace trackit.server.Services.Interfaces
{
    public interface IRequirementService
    {
        Task<RequirementResponseDto> CreateRequirementAsync(RequirementCreateDto requirementDto, string userId);
        Task<bool> ValidateTypeAndCategoryAsync(int typeId, int categoryId);
        Task<RequirementResponseDto> UpdateRequirementAsync(int requirementId, RequirementUpdateDto updateDto, string userId);
        Task<RequirementResponseDto> GetRequirementByIdAsync(int requirementId);
        Task<IEnumerable<RequirementResponseDto>> GetAllRequirementsAsync();
        Task DeleteRequirementAsync(int requirementId);
    }
}
