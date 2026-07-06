namespace Application.Features.FaAssets;

public class FaAssetDto
{
    public long Id { get; set; }
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = null!;
    public string InventoryNumber { get; set; } = null!;
    public string Name { get; set; } = null!;
    public int FaGroupId { get; set; }
    public string FaGroupCode { get; set; } = null!;
    public string FaGroupName { get; set; } = null!;
    public short? OkofId { get; set; }
    public string? OkofCode { get; set; }
    public string? OkofName { get; set; }
    public short DepreciationMethodId { get; set; }
    public string DepreciationMethodCode { get; set; } = null!;
    public string DepreciationMethodName { get; set; } = null!;
    public int UsefulLifeMonths { get; set; }
    public decimal InitialCost { get; set; }
    public decimal SalvageValue { get; set; }
    public DateTime? CommissioningDate { get; set; }
    public DateTime? DeprStartDate { get; set; }
    public decimal? PlannedUnitsTotal { get; set; }
    public int? SourceProductTableId { get; set; }
    public string? SourceProductTableSerialNumber { get; set; }
    public string? SourceProductTableMarkingNumber { get; set; }
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public int? ResponsibleUserId { get; set; }
    public string? ResponsibleUserName { get; set; }
    public short StatusId { get; set; }
    public string StatusCode { get; set; } = null!;
    public string StatusName { get; set; } = null!;
    public short StateId { get; set; }
    public string StateName { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}
