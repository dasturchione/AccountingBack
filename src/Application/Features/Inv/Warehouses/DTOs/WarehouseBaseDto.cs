namespace Application.Features.Warehouses;

public class WarehouseBaseDto
{
    public int? BranchId { get; set; }
    public string? Code { get; set; }
    public string Name { get; set; } = null!;
    public string? Address { get; set; }
    public bool IsMain { get; set; }
    public int? ResponsibleUserId { get; set; }
}
