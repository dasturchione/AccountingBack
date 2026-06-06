namespace Application.Features.Warehouses;

public class WarehouseListDto
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = null!;
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public int? ResponsibleUserId { get; set; }
    public string? ResponsibleUserName { get; set; }
    public short StateId { get; set; }
    public string StateName { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
}
