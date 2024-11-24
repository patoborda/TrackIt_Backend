using Microsoft.EntityFrameworkCore;
using trackit.server.Models;

namespace trackit.server.Repositories.Interfaces
{
    public interface IRequirementRepository
    {
        Task<Requirement> AddAsync(Requirement requirement);
        Task<int> GetNextSequentialNumberAsync();
        Task<bool> ValidateTypeAndCategoryAsync(int typeId, int categoryId);
        Task<string> GetRequirementTypeNameAsync(int typeId);
        Task<string> GetCategoryNameAsync(int categoryId);
        Task<string> GetPriorityNameAsync(int priorityId);

        // Nuevos métodos
        Task<bool> ValidateRequirementExistsAsync(int requirementId); // Validar existencia de requerimiento
        Task AddRequirementRelationAsync(int requirementId, int relatedRequirementId); // Agregar relación
        Task<Requirement> GetByIdAsync(int id);
        Task UpdateAsync(Requirement requirement);
        Task AddUserToRequirementAsync(int requirementId, string userId);
        Task<bool> ValidateUserExistsAsync(string userId);

        Task<bool> ValidateRequirementRelationExistsAsync(int requirementId, int relatedRequirementId);

    }
}
