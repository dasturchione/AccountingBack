namespace Application.Features.Positions;

public class PositionBaseDto
{
    public int OrganizationId { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
}
