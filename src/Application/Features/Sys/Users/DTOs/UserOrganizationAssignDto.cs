namespace Application.Features.Users;

public class UserOrganizationAssignDto
{
    public int OrganizationId { get; set; }
    public int? RoleId { get; set; }
    public bool IsDefault { get; set; }
}
