namespace Application.Features.FaAssets;

public partial class FaAssetBaseDto
{
    public string InventoryNumber { get; set; } = null!;
    public string Name { get; set; } = null!;
    public int FaGroupId { get; set; }
    public short? OkofId { get; set; }
    public short DepreciationMethodId { get; set; }
    public int UsefulLifeMonths { get; set; }
    public decimal InitialCost { get; set; }
    public decimal SalvageValue { get; set; }
    public DateTime? CommissioningDate { get; set; }
    public DateTime? DeprStartDate { get; set; }
    public decimal? PlannedUnitsTotal { get; set; }
    public int? SourceProductTableId { get; set; }
    public int? DepartmentId { get; set; }
    public int? ResponsibleUserId { get; set; }
}
