using trackit.server.Dtos;
using trackit.server.Models;
using trackit.server.Repositories.Interfaces;
using trackit.server.Services.Interfaces;

namespace trackit.server.Services
{
    public class RequirementService : IRequirementService
    {
        private readonly IRequirementRepository _repository;

        public RequirementService(IRequirementRepository repository)
        {
            _repository = repository;
        }

        public async Task<RequirementResponseDto> CreateRequirementAsync(RequirementCreateDto requirementDto)
        {
            // Validar que la categoría pertenece al tipo
            var isValid = await _repository.ValidateTypeAndCategoryAsync(requirementDto.RequirementTypeId, requirementDto.CategoryId);
            if (!isValid)
            {
                throw new ArgumentException("The selected category does not belong to the specified type.");
            }

            // Continuar con la lógica de creación
            var currentYear = DateTime.Now.Year;
            string typeCode = "REH";
            int sequentialNumber = await _repository.GetNextSequentialNumberAsync();
            var generatedCode = $"{typeCode}-{currentYear}-{sequentialNumber.ToString("D10")}";

            var newRequirement = new Requirement
            {
                Subject = requirementDto.Subject,
                Description = requirementDto.Description,
                Code = generatedCode,
                RequirementTypeId = requirementDto.RequirementTypeId,
                CategoryId = requirementDto.CategoryId
            };

            var savedRequirement = await _repository.AddAsync(newRequirement);

            return new RequirementResponseDto
            {
                Id = savedRequirement.Id,
                Subject = savedRequirement.Subject,
                Description = savedRequirement.Description,
                Code = savedRequirement.Code,
                RequirementType = (await _repository.GetRequirementTypeNameAsync(savedRequirement.RequirementTypeId)),
                Category = (await _repository.GetCategoryNameAsync(savedRequirement.CategoryId))
            };
        }

        // Opción: Exponer el método directamente desde el servicio si lo necesitas
        public async Task<bool> ValidateTypeAndCategoryAsync(int typeId, int categoryId)
        {
            return await _repository.ValidateTypeAndCategoryAsync(typeId, categoryId);
        }
    }
}
