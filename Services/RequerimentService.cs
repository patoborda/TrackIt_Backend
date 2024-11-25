﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using trackit.server.Dtos;
using trackit.server.Models;
using trackit.server.Patterns.Observer;
using trackit.server.Repositories.Interfaces;
using trackit.server.Services.Interfaces;

public class RequirementService : IRequirementService
{
    private readonly IRequirementRepository _repository;
    private readonly IUserRepository _userRepository;
    private readonly IRequirementActionService _actionService;
    private readonly RequirementNotifier _notifier; // Notificador

    public RequirementService(
        IRequirementRepository repository,
        IUserRepository userRepository,
        IRequirementActionService actionService,
        RequirementNotifier notifier) // Agregado RequirementNotifier
    {
        _repository = repository;
        _userRepository = userRepository;
        _actionService = actionService;
        _notifier = notifier; // Asignación
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
            var status = requirementDto.AssignedUsers != null && requirementDto.AssignedUsers.Any() ? "Assigned" : "Open";

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

            // Asignar usuarios y enviar notificaciones
            if (requirementDto.AssignedUsers != null)
            {
                foreach (var assignedUserId in requirementDto.AssignedUsers)
                {
                    if (!await _repository.ValidateUserExistsAsync(assignedUserId))
                        throw new ArgumentException($"User {assignedUserId} does not exist.");

                    await _repository.AddUserToRequirementAsync(savedRequirement.Id, assignedUserId);

                    var user = await _userRepository.GetUserByIdAsync(assignedUserId);
                    if (user != null && !string.IsNullOrEmpty(user.Email))
                    {
                        // Notificar al usuario asignado
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

            // Obtener todos los usuarios internos
            var internalUsers = await _userRepository.GetInternalUsersAsync();

            if (internalUsers.Any())
            {
                foreach (var internalUser in internalUsers)
                {
                    await _notifier.NotifyAllAsync("New Requirement Created", new InternalNotificationData
                    {
                        UserIds = internalUsers.Select(u => u.Id).ToList(), // Pasar todos los IDs de usuarios internos
                        Content = $"A new requirement titled '{savedRequirement.Subject}' has been created."
                    });

                }
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
        var requirement = await _repository.GetByIdAsync(requirementId);
        if (requirement == null)
            throw new ArgumentException("Requirement not found.");

        // Registrar acción de eliminación
        await _actionService.LogActionAsync(requirementId, "Deleted", "Requirement deleted.", requirement.CreatedByUserId);

        // Eliminar el requerimiento
        await _repository.DeleteAsync(requirement);
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
            Status = requirement.Status
        };
    }
}
