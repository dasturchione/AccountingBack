namespace Application.Features.ProductGroups;

public class ProductInGroupBaseDto
{
    public string? Code { get; set; }
    public string? Sku { get; set; }
    public string? Mxik { get; set; }
    public string? Article { get; set; }
    public bool IsSold { get; set; } = true;
    public bool IsPurchased { get; set; } = true;
    public short UnitId { get; set; }
    public string? Barcode { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsPieceTracked { get; set; }
    public bool IsService { get; set; }
    public short? DefaultVatRateId { get; set; }
    public decimal? MinStock { get; set; }
}
