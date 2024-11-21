using Microsoft.EntityFrameworkCore;
using trackit.server.Models;
using trackit.server.Repositories.Interfaces;

namespace trackit.server.Repositories
{
    public class RequirementRepository : IRequirementRepository
    {
        private readonly AppDbContext _context;

        public RequirementRepository(AppDbContext context)
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

        // Implementación de GetRequirementTypeNameAsync
        public async Task<string> GetRequirementTypeNameAsync(int typeId)
        {
            var type = await _context.RequirementTypes.FindAsync(typeId);
            return type?.Name ?? "Unknown"; // Devuelve "Unknown" si el tipo no existe
        }

        // Implementación de GetCategoryNameAsync
        public async Task<string> GetCategoryNameAsync(int categoryId)
        {
            var category = await _context.Categories.FindAsync(categoryId);
            return category?.Name ?? "Unknown"; // Devuelve "Unknown" si la categoría no existe
        }
    }
}
