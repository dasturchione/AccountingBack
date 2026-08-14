namespace Application.Features.Manual;

public sealed class FaAssetSelectListDto : SelectListDto
{
    public string InventoryNumber { get; set; } = null!;
    public int FaGroupId { get; set; }
    public string FaGroupName { get; set; } = null!;
    public decimal InitialCost { get; set; }
    public int? AssetAccountId { get; set; }
    public string? AssetAccountNumber { get; set; }
    public string? AssetAccountName { get; set; }
    public short StatusId { get; set; }
    public string StatusName { get; set; } = null!;
}