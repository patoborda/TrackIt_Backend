using trackit.server.Data;
using trackit.server.Models;
using trackit.server.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

public class RequirementTypeRepository : IRequirementTypeRepository
{
    private readonly UserDbContext _context;

    public RequirementTypeRepository(UserDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<RequirementType>> GetAllAsync()
    {
        return await _context.RequirementTypes.ToListAsync();
    }

    public async Task<RequirementType?> GetByIdAsync(int id)
    {
        return await _context.RequirementTypes.FindAsync(id);
    }

    public async Task<RequirementType> AddAsync(RequirementType requirementType)
    {
        _context.RequirementTypes.Add(requirementType);
        await _context.SaveChangesAsync();
        return requirementType;
    }

    public async Task<RequirementType> UpdateAsync(RequirementType requirementType)
    {
        _context.RequirementTypes.Update(requirementType);
        await _context.SaveChangesAsync();
        return requirementType;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var requirementType = await _context.RequirementTypes.FindAsync(id);
        if (requirementType == null)
            return false;

        _context.RequirementTypes.Remove(requirementType);
        await _context.SaveChangesAsync();
        return true;
    }
}
