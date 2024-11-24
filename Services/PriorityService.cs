using trackit.server.Dtos;
using trackit.server.Models;
using trackit.server.Repositories.Interfaces;
using trackit.server.Services.Interfaces;

namespace trackit.server.Services
{
    public class PriorityService : IPriorityService
    {
        private readonly IPriorityRepository _repository;

        public PriorityService(IPriorityRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<PriorityDto>> GetAllAsync()
        {
            var priorities = await _repository.GetAllAsync();
            return priorities.Select(p => new PriorityDto { Id = p.Id, TypePriority = p.TypePriority });
        }

        public async Task<PriorityDto?> GetByIdAsync(int id)
        {
            var priority = await _repository.GetByIdAsync(id);
            if (priority == null) return null;

            return new PriorityDto { Id = priority.Id, TypePriority = priority.TypePriority };
        }

        public async Task<PriorityDto> AddAsync(PriorityDto dto)
        {
            var newPriority = new Priority { TypePriority = dto.TypePriority };
            var createdPriority = await _repository.AddAsync(newPriority);
            return new PriorityDto { Id = createdPriority.Id, TypePriority = createdPriority.TypePriority };
        }

        public async Task<PriorityDto> UpdateAsync(int id, string typePriority)
        {
            var existingPriority = await _repository.GetByIdAsync(id);
            if (existingPriority == null) throw new KeyNotFoundException("Priority not found");

            existingPriority.TypePriority = typePriority;
            var updatedPriority = await _repository.UpdateAsync(existingPriority);
            return new PriorityDto { Id = updatedPriority.Id, TypePriority = updatedPriority.TypePriority };
        }


        public async Task<bool> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }
    }
}
