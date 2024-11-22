using trackit.server.Dtos;
using trackit.server.Models;
using trackit.server.Repositories.Interfaces;
using trackit.server.Services.Interfaces;

namespace trackit.server.Services
{
    public class RequirementService : IRequirementService
    {
        private readonly IRequirementRepository _repository;
        private readonly RequirementNotifier _notifier; // Patrón Observer

        public RequirementService(IRequirementRepository repository, RequirementNotifier notifier)
        {
            _repository = repository;
            _notifier = notifier;
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
                PriorityId = requirementDto.PriorityId, // Puede ser null
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

            // Notificar a los observadores (usando el patrón Observer)
            _notifier.NotifyObservers(savedRequirement, "Creado", null, "Requirement created successfully.");

            // Preparar la respuesta
            return new RequirementResponseDto
            {
                Id = savedRequirement.Id,
                Subject = savedRequirement.Subject,
                Description = savedRequirement.Description,
                Code = savedRequirement.Code,
                RequirementType = await _repository.GetRequirementTypeNameAsync(savedRequirement.RequirementTypeId),
                Category = await _repository.GetCategoryNameAsync(savedRequirement.CategoryId),
                Priority = savedRequirement.PriorityId.HasValue
                    ? await _repository.GetPriorityNameAsync(savedRequirement.PriorityId.Value)
                    : null,
                Date = savedRequirement.Date,
                Status = savedRequirement.Status
            };
        }

        public async Task<RequirementResponseDto> UpdateRequirementAsync(int requirementId, RequirementUpdateDto updateDto, string userId)
        {
            // Obtener el requerimiento
            var requirement = await _repository.GetByIdAsync(requirementId);
            if (requirement == null)
            {
                throw new ArgumentException("Requirement not found.");
            }

            // Registrar los cambios
            var details = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(updateDto.Subject) && updateDto.Subject != requirement.Subject)
            {
                details.Add("Subject (Before)", requirement.Subject);
                details.Add("Subject (After)", updateDto.Subject);
                requirement.Subject = updateDto.Subject;
            }

            if (!string.IsNullOrEmpty(updateDto.Description) && updateDto.Description != requirement.Description)
            {
                details.Add("Description (Before)", requirement.Description);
                details.Add("Description (After)", updateDto.Description);
                requirement.Description = updateDto.Description;
            }

            if (updateDto.PriorityId.HasValue && updateDto.PriorityId != requirement.PriorityId)
            {
                var oldPriority = requirement.PriorityId.HasValue
                    ? await _repository.GetPriorityNameAsync(requirement.PriorityId.Value)
                    : "None";
                var newPriority = await _repository.GetPriorityNameAsync(updateDto.PriorityId.Value);

                details.Add("Priority (Before)", oldPriority);
                details.Add("Priority (After)", newPriority);
                requirement.PriorityId = updateDto.PriorityId.Value;
            }

            if (!string.IsNullOrEmpty(updateDto.Status) && updateDto.Status != requirement.Status)
            {
                details.Add("Status (Before)", requirement.Status);
                details.Add("Status (After)", updateDto.Status);
                requirement.Status = updateDto.Status;
            }

            // Guardar los cambios
            await _repository.UpdateAsync(requirement);

            // Notificar a los observadores (usando el patrón Observer)
            _notifier.NotifyObservers(requirement, "Modificado", userId, string.Join(", ", details.Select(d => $"{d.Key}: {d.Value}")));

            // Preparar y devolver la respuesta
            return new RequirementResponseDto
            {
                Id = requirement.Id,
                Subject = requirement.Subject,
                Description = requirement.Description,
                Code = requirement.Code,
                RequirementType = await _repository.GetRequirementTypeNameAsync(requirement.RequirementTypeId),
                Category = await _repository.GetCategoryNameAsync(requirement.CategoryId),
                Priority = requirement.PriorityId.HasValue
                    ? await _repository.GetPriorityNameAsync(requirement.PriorityId.Value)
                    : null,
                Date = requirement.Date,
                Status = requirement.Status
            };
        }

        // Método para validar si un tipo y categoría son válidos
        public async Task<bool> ValidateTypeAndCategoryAsync(int typeId, int categoryId)
        {
            return await _repository.ValidateTypeAndCategoryAsync(typeId, categoryId);
        }
    }
}
