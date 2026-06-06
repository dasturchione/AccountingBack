namespace Application.Features.Warehouses;

public class WarehouseBaseDto
{
    public int OrganizationId { get; set; }
    public int? BranchId { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public int? ResponsibleUserId { get; set; }
}
