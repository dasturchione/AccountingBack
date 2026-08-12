namespace Application.Features.FaAssets;

public class FaAssetUpdateDto
{
    public string InventoryNumber { get; set; } = null!;
    public string Name { get; set; } = null!;
    public int FaGroupId { get; set; }
    public short? OkofId { get; set; }
    public short DepreciationMethodId { get; set; }
    public int UsefulLifeMonths { get; set; }
    public decimal? PlannedUnitsTotal { get; set; }
    public int? AssetAccountId { get; set; }
    public int? AccumulatedDepreciationAccountId { get; set; }
    public int? DepreciationExpenseAccountId { get; set; }
}