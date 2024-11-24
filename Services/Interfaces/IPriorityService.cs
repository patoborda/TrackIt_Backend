using trackit.server.Dtos;

public interface IPriorityService
{
    Task<IEnumerable<PriorityDto>> GetAllAsync();
    Task<PriorityDto?> GetByIdAsync(int id);
    Task<PriorityDto> AddAsync(PriorityDto dto);
    Task<PriorityDto> UpdateAsync(int id, string typePriority); // Cambiado aquí
    Task<bool> DeleteAsync(int id);
}
