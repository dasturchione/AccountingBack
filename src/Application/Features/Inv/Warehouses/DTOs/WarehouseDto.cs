namespace Application.Features.Warehouses;

public class WarehouseDto
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = null!;
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }
    public string? Code { get; set; }
    public string Name { get; set; } = null!;
    public string? Address { get; set; }
    public bool IsMain { get; set; }
    public int? ResponsibleUserId { get; set; }
    public string? ResponsibleUserName { get; set; }
    public short StateId { get; set; }
    public string StateName { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
}
