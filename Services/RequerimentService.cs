using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using trackit.server.Dtos;
using trackit.server.Models;
using trackit.server.Patterns.Observer;
using trackit.server.Repositories;
using trackit.server.Repositories.Interfaces;
using trackit.server.Services.Interfaces;

public class RequirementService : IRequirementService
{
    private readonly IRequirementRepository _repository;
    private readonly IUserRepository _userRepository;
    private readonly IRequirementActionService _actionService;
    private readonly INotificationRepository _notificationRepository;
    private readonly RequirementNotifier _notifier;

    public RequirementService(
        IRequirementRepository repository,
        IUserRepository userRepository,
        IRequirementActionService actionService,
        INotificationRepository notificationRepository,
        RequirementNotifier notifier)
    {
        _repository = repository;
        _userRepository = userRepository;
        _actionService = actionService;
        _notificationRepository = notificationRepository;
        _notifier = notifier;
    }

    public async Task<RequirementResponseDto> CreateRequirementAsync(RequirementCreateDto requirementDto, string userId)
    {
        try
        {
            // Validar Tipo y Categoría
            if (!await _repository.ValidateTypeAndCategoryAsync(requirementDto.RequirementTypeId, requirementDto.CategoryId))
                throw new ArgumentException("The selected category does not belong to the specified type.");

            // Crear código único para el requerimiento
            var nextSequentialNumber = await _repository.GetNextSequentialNumberAsync();
            var currentYear = DateTime.UtcNow.Year;
            var requirementCode = $"REH-{currentYear}-{nextSequentialNumber:00000000}";

            // Determinar estado inicial
            var status = requirementDto.AssignedUsers != null && requirementDto.AssignedUsers.Any() ? "Asignado" : "Abierto";

            // Crear requerimiento
            var newRequirement = new Requirement
            {
                Subject = requirementDto.Subject,
                Code = requirementCode,
                Description = requirementDto.Description,
                RequirementTypeId = requirementDto.RequirementTypeId,
                CategoryId = requirementDto.CategoryId,
                PriorityId = requirementDto.PriorityId,
                Status = status,
                Date = DateTime.UtcNow,
                CreatedByUserId = userId
            };

            var savedRequirement = await _repository.AddAsync(newRequirement);

            // Crear notificación general para el requerimiento
            var notification = new Notification
            {
                Message = $"New requirement created: {savedRequirement.Subject}",
                Timestamp = DateTime.UtcNow,
                IsRead = false
            };
            await _notificationRepository.AddNotificationAsync(notification);

            // Asociar usuarios asignados a la notificación y enviar notificaciones
            if (requirementDto.AssignedUsers != null)
            {
                foreach (var assignedUserId in requirementDto.AssignedUsers)
                {
                    if (!await _repository.ValidateUserExistsAsync(assignedUserId))
                        throw new ArgumentException($"User {assignedUserId} does not exist.");

                    await _repository.AddUserToRequirementAsync(savedRequirement.Id, assignedUserId);

                    var existingNotification = await _notificationRepository.GetUserNotificationAsync(assignedUserId, notification.Id);
                    if (existingNotification == null)
                    {
                        await _notificationRepository.AddUserNotificationAsync(assignedUserId, notification.Id);
                    }

                    // Notificar al usuario asignado por email
                    var user = await _userRepository.GetUserByIdAsync(assignedUserId);
                    if (user != null && !string.IsNullOrEmpty(user.Email))
                    {
                        await _notifier.NotifyAllAsync("Requirement Assigned", new EmailNotificationData
                        {
                            Email = user.Email,
                            Content = $"You have been assigned to the requirement: {savedRequirement.Subject}."
                        });
                    }
                    else
                    {
                        Console.WriteLine($"Assigned user {assignedUserId} has no valid email.");
                    }
                }
            }

            // Notificar a todos los usuarios internos sobre el nuevo requerimiento
            var internalUsers = await _userRepository.GetInternalUsersAsync();
            if (internalUsers.Any())
            {
                foreach (var internalUser in internalUsers)
                {
                    var existingNotification = await _notificationRepository.GetUserNotificationAsync(internalUser.Id, notification.Id);
                    if (existingNotification == null)
                    {
                        await _notificationRepository.AddUserNotificationAsync(internalUser.Id, notification.Id);
                    }
                }

                await _notifier.NotifyAllAsync("New Requirement Created", new InternalNotificationData
                {
                    UserIds = internalUsers.Select(u => u.Id).ToList(),
                    Content = $"A new requirement titled '{savedRequirement.Subject}' has been created."
                });
            }
            else
            {
                Console.WriteLine("No internal users found to notify.");
            }

            // Registrar acción de creación
            await _actionService.LogActionAsync(savedRequirement.Id, "Created", "Requirement created successfully.", userId);

            return await MapToResponseDtoAsync(savedRequirement);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating requirement: {ex.Message}");
            throw;
        }
    }
    public async Task<RequirementResponseDto> UpdateRequirementAsync(int requirementId, RequirementUpdateDto updateDto, string userId)
        {
            try
            {
                var requirement = await _repository.GetByIdAsync(requirementId);
                if (requirement == null)
                    throw new ArgumentException("Requirement not found.");

                var details = new Dictionary<string, string>();

                // Actualizar campos y capturar detalles
                if (!string.IsNullOrEmpty(updateDto.Subject) && updateDto.Subject != requirement.Subject)
                {
                    details.Add("Subject", $"{requirement.Subject} → {updateDto.Subject}");
                    requirement.Subject = updateDto.Subject;
                }

                if (!string.IsNullOrEmpty(updateDto.Description) && updateDto.Description != requirement.Description)
                {
                    details.Add("Description", $"{requirement.Description} → {updateDto.Description}");
                    requirement.Description = updateDto.Description;
                }

                await _repository.UpdateAsync(requirement);

                // Notificar usuarios asignados sobre la actualización
                var assignedUsers = await _repository.GetAssignedUsersAsync(requirement.Id);
                foreach (var user in assignedUsers)
                {
                    if (!string.IsNullOrEmpty(user.Email))
                    {
                        await _notifier.NotifyAllAsync("Requirement Updated", new EmailNotificationData
                        {
                            Email = user.Email,
                            Content = $"The requirement '{requirement.Subject}' has been updated. Details:\n" +
                                      string.Join("\n", details.Select(d => $"{d.Key}: {d.Value}"))
                        });
                    }
                }

                // Registrar acción de actualización
                await _actionService.LogActionAsync(requirement.Id, "Updated", string.Join("; ", details.Select(d => $"{d.Key}: {d.Value}")), userId);

                return await MapToResponseDtoAsync(requirement);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating requirement: {ex.Message}");
                throw;
            }
        }
    public async Task<bool> ValidateTypeAndCategoryAsync(int typeId, int categoryId)
        {
            return await _repository.ValidateTypeAndCategoryAsync(typeId, categoryId);
        }
    public async Task<RequirementResponseDto> GetRequirementByIdAsync(int requirementId)
        {
            var requirement = await _repository.GetByIdAsync(requirementId);
            if (requirement == null)
                throw new ArgumentException("Requirement not found.");

            return await MapToResponseDtoAsync(requirement);
        }
    public async Task<IEnumerable<RequirementResponseDto>> GetAllRequirementsAsync()
        {
            var requirements = await _repository.GetAllAsync();
            var response = new List<RequirementResponseDto>();

            foreach (var requirement in requirements)
            {
                response.Add(await MapToResponseDtoAsync(requirement));
            }

            return response;
        }
    public async Task DeleteRequirementAsync(int requirementId)
    {
        // Validar que el requerimiento exista
        var requirement = await _repository.GetByIdAsync(requirementId);
        if (requirement == null)
            throw new ArgumentException("Requirement not found.");

        // Registrar la acción en el log
        await _actionService.LogActionAsync(
            requirementId,
            "Deleted",
            $"Requirement '{requirement.Code}' marked as deleted.",
            requirement.CreatedByUserId
        );

        // Marcar como eliminado
        requirement.IsDeleted = true;
        await _repository.UpdateAsync(requirement);
    }
    private async Task<RequirementResponseDto> MapToResponseDtoAsync(Requirement requirement)
    {
        return new RequirementResponseDto
        {
            Id = requirement.Id,
            Subject = requirement.Subject,
            Code = requirement.Code,
            Description = requirement.Description,
            RequirementType = await _repository.GetRequirementTypeNameAsync(requirement.RequirementTypeId),
            Category = await _repository.GetCategoryNameAsync(requirement.CategoryId),
            Priority = requirement.PriorityId.HasValue
                ? await _repository.GetPriorityNameAsync(requirement.PriorityId.Value)
                : null,
            Date = requirement.Date,
            Status = requirement.Status,
            CreatedByUserId = requirement.CreatedByUserId
        };
    }
    public async Task<IEnumerable<RequirementResponseDto>> GetDeletedRequirementsAsync()
    {
        var deletedRequirements = await _repository.GetAllRequirementsEliminatedAsync();

        return deletedRequirements.Select(req => new RequirementResponseDto
        {
            Id = req.Id,
            Subject = req.Subject,
            Code = req.Code,
            Description = req.Description,
            RequirementType = req.RequirementType.Name,
            Category = req.Category.Name,
            Priority = req.PriorityId.HasValue ? req.Priority.TypePriority : null,
            Date = req.Date,
            Status = req.Status
        }).ToList();
    }
    public async Task RestoreRequirementAsync(int requirementId)
    {
        // Ignorar el filtro global para encontrar requerimientos eliminados
        var requirement = await _repository.GetByIdIgnoringFiltersAsync(requirementId);
        if (requirement == null || !requirement.IsDeleted)
        {
            throw new ArgumentException("Requirement not found or is not deleted.");
        }

        // Registrar la acción en el log
        await _actionService.LogActionAsync(
            requirementId,
            "Deleted",
            $"Requirement '{requirement.Code}' marked as restored.",
            requirement.CreatedByUserId
        );

        // Restaurar el requerimiento
        requirement.IsDeleted = false;
        await _repository.UpdateAsync(requirement);
    }
    public async Task<List<RequirementResponseDto>> GetAssignedRequirementsByUserIdAsync(string userId)
    {
        var requirements = await _repository.GetAssignedRequirementsByUserIdAsync(userId);

        return requirements.Select(r => new RequirementResponseDto
        {
            Id = r.Id,
            Code = r.Code,
            Subject = r.Subject,
            Description = r.Description,
            RequirementType = r.RequirementType?.Name,
            Category = r.Category?.Name,
            Priority = r.Priority?.TypePriority,
            Status = r.Status,
            Date = r.Date
        }).ToList();
    }
    public async Task<List<UserProfileDto>> GetUsersAssignedToRequirementAsync(int requirementId)
    {
        var users = await _repository.GetUsersAssignedToRequirementAsync(requirementId);

        return users.Select(u => new UserProfileDto
        {
            Id = u.Id,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Email = u.Email,
            UserName = u.UserName,
            Image = u.Image,
            IsEnabled = u.IsEnabled
        }).ToList();
    }
    public async Task<List<RequirementResponseWithUsersDto>> GetAllRequirementsWithUsersAsync()
    {
        var requirements = await _repository.GetRequirementsWithAssignedUsersAsync();

        var response = requirements.Select(r => new RequirementResponseWithUsersDto
        {
            Requirement = new RequirementResponseDto
            {
                Id = r.Id,
                Subject = r.Subject,
                Code = r.Code,
                Description = r.Description,
                RequirementType = r.RequirementType?.Name,
                Category = r.Category?.Name,
                Priority = r.Priority?.TypePriority,
                Status = r.Status,
                Date = r.Date
            },
            AssignedUsers = r.UserRequirements.Select(ur => new UserProfileDto
            {
                Id = ur.User.Id,
                FirstName = ur.User.FirstName,
                LastName = ur.User.LastName,
                Email = ur.User.Email,
                UserName = ur.User.UserName,
                Image = ur.User.Image ?? "https://example.com/default-image.png",
                IsEnabled = ur.User.IsEnabled
            }).ToList()
        }).ToList();

        return response;
    }


        // Método para obtener los requerimientos creados por un usuario
        public async Task<List<RequirementResponseDto>> GetRequirementsCreatedByUserIdAsync(string userId)
    {
        var requirements = await _repository.GetRequirementsCreatedByUserIdAsync(userId);

        return requirements.Select(r => new RequirementResponseDto
        {
            Id = r.Id,
            Subject = r.Subject,
            Code = r.Code,
            Description = r.Description,
            RequirementType = r.RequirementType?.Name,
            Category = r.Category?.Name,
            Priority = r.Priority?.TypePriority,
            Status = r.Status,
            Date = r.Date,
            CreatedByUserId= r.CreatedByUserId

        }).ToList();
    }

}
