using trackit.server.Dtos;
using trackit.server.Models;
using trackit.server.Repositories.Interfaces;
using trackit.server.Services.Interfaces;

namespace trackit.server.Services
{
    public class RequirementService : IRequirementService
    {
        private readonly IRequirementRepository _repository;
        private readonly RequirementNotifier _notifier;

        public RequirementService(IRequirementRepository repository, RequirementNotifier notifier)
        {
            _repository = repository;
            _notifier = notifier;
        }

        public async Task<RequirementResponseDto> CreateRequirementAsync(RequirementCreateDto requirementDto, string userId)
        {
            try
            {
                // Validar que la categoría pertenece al tipo
                var isValid = await _repository.ValidateTypeAndCategoryAsync(requirementDto.RequirementTypeId, requirementDto.CategoryId);
                if (!isValid)
                {
                    throw new ArgumentException("The selected category does not belong to the specified type.");
                }

                // Generar código único para el requerimiento
                var nextSequentialNumber = await _repository.GetNextSequentialNumberAsync();
                var currentYear = DateTime.UtcNow.Year;
                var requirementCode = $"REH-{currentYear}-{nextSequentialNumber:00000000}";

                // Determinar el estado inicial
                var status = requirementDto.AssignedUsers != null && requirementDto.AssignedUsers.Any() ? "Asignado" : "Abierto";

                // Crear el requerimiento
                var newRequirement = new Requirement
                {
                    Subject = requirementDto.Subject,
                    Code = requirementCode,
                    Description = requirementDto.Description,
                    RequirementTypeId = requirementDto.RequirementTypeId,
                    CategoryId = requirementDto.CategoryId,
                    PriorityId = requirementDto.PriorityId,
                    Status = status,
                    Date = DateTime.UtcNow
                };

                // Guardar el requerimiento
                var savedRequirement = await _repository.AddAsync(newRequirement);

                if (requirementDto.AssignedUsers != null && requirementDto.AssignedUsers.Any())
                {
                    foreach (var assignedUserId in requirementDto.AssignedUsers)
                    {
                        var userExists = await _repository.ValidateUserExistsAsync(assignedUserId);
                        Console.WriteLine($"Validating user {assignedUserId}: Exists? {userExists}");
                        if (!userExists)
                        {
                            throw new ArgumentException($"The user {assignedUserId} does not exist.");
                        }
                        await _repository.AddUserToRequirementAsync(savedRequirement.Id, assignedUserId);
                    }
                }


                // Manejar requerimientos relacionados
                if (requirementDto.RelatedRequirementIds != null && requirementDto.RelatedRequirementIds.Any())
                {
                    foreach (var relatedId in requirementDto.RelatedRequirementIds)
                    {
                        var exists = await _repository.ValidateRequirementExistsAsync(relatedId);
                        if (exists)
                        {
                            var relationExists = await _repository.ValidateRequirementRelationExistsAsync(savedRequirement.Id, relatedId);
                            if (!relationExists)
                            {
                                await _repository.AddRequirementRelationAsync(savedRequirement.Id, relatedId);
                            }
                        }
                    }
                }

                // Notificar a los observadores
                _notifier.NotifyObservers(savedRequirement, "Creado", userId, "Requirement created successfully.");

                // Preparar la respuesta
                return new RequirementResponseDto
                {
                    Id = savedRequirement.Id,
                    Subject = savedRequirement.Subject,
                    Code = savedRequirement.Code,
                    Description = savedRequirement.Description,
                    RequirementType = await _repository.GetRequirementTypeNameAsync(savedRequirement.RequirementTypeId),
                    Category = await _repository.GetCategoryNameAsync(savedRequirement.CategoryId),
                    Priority = savedRequirement.PriorityId.HasValue
                        ? await _repository.GetPriorityNameAsync(savedRequirement.PriorityId.Value)
                        : "Sin prioridad",
                    Date = savedRequirement.Date,
                    Status = savedRequirement.Status
                };
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Validation error: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear requerimiento: {ex.Message}");
                throw;
            }
        }

        public async Task<RequirementResponseDto> UpdateRequirementAsync(int requirementId, RequirementUpdateDto updateDto, string userId)
        {
            var requirement = await _repository.GetByIdAsync(requirementId);
            if (requirement == null)
            {
                throw new ArgumentException("Requirement not found.");
            }

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
                    : "Sin prioridad";
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

            await _repository.UpdateAsync(requirement);
            _notifier.NotifyObservers(requirement, "Modificado", userId, string.Join(", ", details.Select(d => $"{d.Key}: {d.Value}")));

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

        public async Task<bool> ValidateTypeAndCategoryAsync(int typeId, int categoryId)
        {
            return await _repository.ValidateTypeAndCategoryAsync(typeId, categoryId);
        }
    }
}
