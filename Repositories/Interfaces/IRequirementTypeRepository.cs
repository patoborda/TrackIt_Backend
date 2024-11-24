using trackit.server.Models;

namespace trackit.server.Repositories.Interfaces
{
    public interface IRequirementTypeRepository
    {
        Task<IEnumerable<RequirementType>> GetAllAsync();
        Task<RequirementType?> GetByIdAsync(int id);
        Task<RequirementType> AddAsync(RequirementType requirementType);
        Task<RequirementType> UpdateAsync(RequirementType requirementType);
        Task<bool> DeleteAsync(int id);
    }


}
