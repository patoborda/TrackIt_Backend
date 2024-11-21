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

            // Crear el código
            var currentYear = DateTime.Now.Year;
            string typeCode = "REH";
            int sequentialNumber = await _repository.GetNextSequentialNumberAsync();
            var generatedCode = $"{typeCode}-{currentYear}-{sequentialNumber.ToString("D10")}";

            // Crear el requerimiento
            var newRequirement = new Requirement
            {
                Subject = requirementDto.Subject,
                Description = requirementDto.Description,
                Code = generatedCode,
                RequirementTypeId = requirementDto.RequirementTypeId,
                CategoryId = requirementDto.CategoryId,
                PriorityId = requirementDto.PriorityId,
                Date = DateTime.UtcNow, // Fecha actual
                Status = "Abierto" // Estado predeterminado
            };

            // Guardar el requerimiento en la base de datos
            var savedRequirement = await _repository.AddAsync(newRequirement);


            // Manejar requerimientos relacionados
            if (requirementDto.RelatedRequirementIds != null && requirementDto.RelatedRequirementIds.Any())
            {
                foreach (var relatedId in requirementDto.RelatedRequirementIds)
                {
                    // Verificar que el requerimiento relacionado no sea el mismo que el creado
                    if (relatedId == savedRequirement.Id)
                    {
                        throw new ArgumentException($"A requirement cannot be related to itself. RequirementId: {savedRequirement.Id}");
                    }

                    // Verificar que el requerimiento relacionado exista
                    var exists = await _repository.ValidateRequirementExistsAsync(relatedId);
                    if (exists)
                    {
                        await _repository.AddRequirementRelationAsync(savedRequirement.Id, relatedId);
                    }
                    else
                    {
                        throw new ArgumentException($"Related Requirement with ID {relatedId} does not exist.");
                    }
                }
            }


            // Preparar la respuesta
            return new RequirementResponseDto
            {
                Id = savedRequirement.Id,
                Subject = savedRequirement.Subject,
                Description = savedRequirement.Description,
                Code = savedRequirement.Code,
                RequirementType = await _repository.GetRequirementTypeNameAsync(savedRequirement.RequirementTypeId),
                Category = await _repository.GetCategoryNameAsync(savedRequirement.CategoryId),
                Priority = await _repository.GetPriorityNameAsync(savedRequirement.PriorityId),
                Date = savedRequirement.Date,
                Status = savedRequirement.Status
            };
        }

        // Método para validar si un tipo y categoría son válidos
        public async Task<bool> ValidateTypeAndCategoryAsync(int typeId, int categoryId)
        {
            return await _repository.ValidateTypeAndCategoryAsync(typeId, categoryId);
        }
    }
}
