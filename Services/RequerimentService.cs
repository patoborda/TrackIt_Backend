using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using trackit.server.Dtos;
using trackit.server.Models;
using trackit.server.Repositories.Interfaces;
using trackit.server.Services.Interfaces;

public class RequirementService : IRequirementService
{
    private readonly IRequirementRepository _repository;
    private readonly IEmailService _emailService;
    private readonly IUserRepository _userRepository;
    private readonly IRequirementActionService _actionService;

    public RequirementService(
        IRequirementRepository repository,
        IEmailService emailService,
        IUserRepository userRepository,
        IRequirementActionService actionService)
    {
        _repository = repository;
        _emailService = emailService;
        _userRepository = userRepository;
        _actionService = actionService;
    }

    public async Task<RequirementResponseDto> CreateRequirementAsync(RequirementCreateDto requirementDto, string userId)
    {
        try
        {
            // Validación y creación del requerimiento
            if (!await _repository.ValidateTypeAndCategoryAsync(requirementDto.RequirementTypeId, requirementDto.CategoryId))
                throw new ArgumentException("The selected category does not belong to the specified type.");

            var nextSequentialNumber = await _repository.GetNextSequentialNumberAsync();
            var currentYear = DateTime.UtcNow.Year;
            var requirementCode = $"REH-{currentYear}-{nextSequentialNumber:00000000}";

            var status = requirementDto.AssignedUsers != null && requirementDto.AssignedUsers.Any() ? "Assigned" : "Open";

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

            // Asignar usuarios al requerimiento y enviar notificaciones
            if (requirementDto.AssignedUsers != null)
            {
                foreach (var assignedUserId in requirementDto.AssignedUsers)
                {
                    if (!await _repository.ValidateUserExistsAsync(assignedUserId))
                        throw new ArgumentException($"User {assignedUserId} does not exist.");

                    await _repository.AddUserToRequirementAsync(savedRequirement.Id, assignedUserId);

                    // Obtener el email del usuario asignado y enviar notificación
                    var user = await _userRepository.GetUserByIdAsync(assignedUserId);
                    if (!string.IsNullOrEmpty(user?.Email))
                    {
                        var subject = $"New Requirement Assigned: {savedRequirement.Subject}";
                        var message = $@"
                            <p>Hello {user.FirstName},</p>
                            <p>You have been assigned to the requirement <strong>{savedRequirement.Subject}</strong>.</p>
                            <p>Details: {savedRequirement.Description}</p>";

                        await _emailService.SendEmailAsync(user.Email, subject, message);
                    }
                }
            }

            // Registrar acción
            await _actionService.LogActionAsync(savedRequirement.Id, "Created", "Requirement created successfully.", userId);

            return await MapToResponseDtoAsync(savedRequirement);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            throw;
        }
    }

    public async Task<RequirementResponseDto> UpdateRequirementAsync(int requirementId, RequirementUpdateDto updateDto, string userId)
    {
        var requirement = await _repository.GetByIdAsync(requirementId);
        if (requirement == null)
            throw new ArgumentException("Requirement not found.");

        var details = new Dictionary<string, string>();

        // Actualización de campos y recopilación de detalles para notificaciones
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

        // Enviar notificaciones a los usuarios asignados
        var assignedUsers = await _repository.GetAssignedUsersAsync(requirement.Id);
        foreach (var user in assignedUsers)
        {
            if (!string.IsNullOrEmpty(user.Email))
            {
                var subject = $"Requirement Updated: {requirement.Subject}";
                var message = $@"
                    <p>The requirement <strong>{requirement.Subject}</strong> has been updated.</p>
                    <p>Details:</p>
                    <ul>
                        {string.Join("", details.Select(d => $"<li>{d.Key}: {d.Value}</li>"))}
                    </ul>";

                await _emailService.SendEmailAsync(user.Email, subject, message);
            }
        }

        // Registrar acción
        await _actionService.LogActionAsync(requirement.Id, "Updated", string.Join("; ", details.Select(d => $"{d.Key}: {d.Value}")), userId);

        return await MapToResponseDtoAsync(requirement);
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
