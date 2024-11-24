using Microsoft.EntityFrameworkCore;
using trackit.server.Models;
using trackit.server.Repositories.Interfaces;
using trackit.server.Data;

namespace trackit.server.Repositories
{
    public class RequirementRepository : IRequirementRepository
    {
        private readonly UserDbContext _context;

        public RequirementRepository(UserDbContext context)
        {
            _context = context;
        }

        public async Task<Requirement> AddAsync(Requirement requirement)
        {
            _context.Requirements.Add(requirement);
            await _context.SaveChangesAsync();
            return requirement;
        }

        public async Task<int> GetNextSequentialNumberAsync()
        {
            return await _context.Requirements.CountAsync() + 1;
        }

        public async Task<bool> ValidateTypeAndCategoryAsync(int typeId, int categoryId)
        {
            return await _context.Categories.AnyAsync(c => c.Id == categoryId && c.RequirementTypeId == typeId);
        }

        public async Task<string> GetRequirementTypeNameAsync(int typeId)
        {
            var type = await _context.RequirementTypes.FindAsync(typeId);
            return type?.Name ?? "Unknown";
        }

        public async Task<string> GetCategoryNameAsync(int categoryId)
        {
            var category = await _context.Categories.FindAsync(categoryId);
            return category?.Name ?? "Unknown";
        }

        public async Task<string> GetPriorityNameAsync(int priorityId)
        {
            var priority = await _context.Priorities.FindAsync(priorityId);
            return priority?.TypePriority ?? "Unknown";
        }

        public async Task<bool> ValidateRequirementExistsAsync(int requirementId)
        {
            return await _context.Requirements.AnyAsync(r => r.Id == requirementId);
        }


        public async Task<bool> ValidateRequirementRelationExistsAsync(int requirementId, int relatedRequirementId)
        {
            return await _context.RequirementRelations
                .AnyAsync(rr => rr.RequirementId == requirementId && rr.RelatedRequirementId == relatedRequirementId);
        }


        public async Task AddRequirementRelationAsync(int requirementId, int relatedRequirementId)
        {
            var relation = new RequirementRelation
            {
                RequirementId = requirementId,
                RelatedRequirementId = relatedRequirementId
            };
            _context.RequirementRelations.Add(relation);
            await _context.SaveChangesAsync();
        }

        public async Task<Requirement> GetByIdAsync(int id)
        {
            return await _context.Requirements.FindAsync(id);
        }

        public async Task UpdateAsync(Requirement requirement)
        {
            _context.Requirements.Update(requirement);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(Requirement requirement)
        {
            _context.Requirements.Remove(requirement);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUserAssignmentsAsync(int requirementId)
        {
            var userAssignments = _context.UserRequirements.Where(ur => ur.RequirementId == requirementId);
            _context.UserRequirements.RemoveRange(userAssignments);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteRequirementRelationsAsync(int requirementId)
        {
            var relations = _context.RequirementRelations.Where(rr => rr.RequirementId == requirementId || rr.RelatedRequirementId == requirementId);
            _context.RequirementRelations.RemoveRange(relations);
            await _context.SaveChangesAsync();
        }

        public async Task AddUserToRequirementAsync(int requirementId, string userId)
        {
            // Validación para evitar duplicados
            var exists = await _context.UserRequirements
                .AnyAsync(ur => ur.UserId == userId && ur.RequirementId == requirementId);

            if (!exists)
            {
                var userRequirement = new UserRequirement
                {
                    UserId = userId,
                    RequirementId = requirementId
                };

                _context.UserRequirements.Add(userRequirement);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<IEnumerable<Requirement>> GetAllAsync()
        {
            return await _context.Requirements
                .Include(r => r.RequirementType)
                .Include(r => r.Category)
                .Include(r => r.Priority)
                .ToListAsync();
        }


        public async Task<bool> ValidateUserExistsAsync(string userId)
        {
            return await _context.Users.AnyAsync(u => u.Id == userId);
        }
    }
}
