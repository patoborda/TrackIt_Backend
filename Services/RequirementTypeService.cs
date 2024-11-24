using trackit.server.Dtos;
using trackit.server.Models;
using trackit.server.Repositories.Interfaces;
using trackit.server.Services.Interfaces;

namespace trackit.server.Services
{
    public class RequirementTypeService : IRequirementTypeService
    {
        private readonly IRequirementTypeRepository _repository;

        public RequirementTypeService(IRequirementTypeRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<RequirementTypeDto>> GetAllAsync()
        {
            var types = await _repository.GetAllAsync();
            return types.Select(t => new RequirementTypeDto { Id = t.Id, Name = t.Name });
        }

        public async Task<RequirementTypeDto?> GetByIdAsync(int id)
        {
            var type = await _repository.GetByIdAsync(id);
            if (type == null) return null;

            return new RequirementTypeDto { Id = type.Id, Name = type.Name };
        }

        public async Task<RequirementTypeDto> AddAsync(RequirementTypeDto dto)
        {
            var newType = new RequirementType { Name = dto.Name };
            var createdType = await _repository.AddAsync(newType);
            return new RequirementTypeDto { Id = createdType.Id, Name = createdType.Name };
        }

        public async Task<RequirementTypeDto> UpdateAsync(int id, string name)
        {
            var existingType = await _repository.GetByIdAsync(id);
            if (existingType == null) throw new KeyNotFoundException("RequirementType not found");

            existingType.Name = name;

            var updatedType = await _repository.UpdateAsync(existingType);
            return new RequirementTypeDto
            {
                Id = updatedType.Id,
                Name = updatedType.Name
            };
        }


        public async Task<bool> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }
    }

}
