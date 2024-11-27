using trackit.server.Dtos;

public class RequirementResponseWithUsersDto
{
    public RequirementResponseDto Requirement { get; set; }
    public List<UserProfileDto> AssignedUsers { get; set; }
}
