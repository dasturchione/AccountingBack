namespace Application.Features.FaAssets;

public class FaAssetUpdateDto
{
    public string InventoryNumber { get; set; } = null!;
    public string Name { get; set; } = null!;
    public int FaGroupId { get; set; }
    public short? OkofId { get; set; }
}
