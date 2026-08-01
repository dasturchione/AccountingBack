namespace Application.Features.FaAssets;

public partial class FaAssetListDto
{
    public long Id { get; set; }
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = null!;
    public string InventoryNumber { get; set; } = null!;
    public string Name { get; set; } = null!;
    public int FaGroupId { get; set; }
    public string FaGroupName { get; set; } = null!;
    public short DepreciationMethodId { get; set; }
    public string DepreciationMethodName { get; set; } = null!;
    public decimal InitialCost { get; set; }
    public DateTime? CommissioningDate { get; set; }
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public int? ResponsibleUserId { get; set; }
    public string? ResponsibleUserName { get; set; }
    public short StatusId { get; set; }
    public string StatusName { get; set; } = null!;
    public short StateId { get; set; }
    public string StateName { get; set; } = null!;
    public DateTime UpdatedDate { get; set; }
}
