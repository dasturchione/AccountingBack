namespace Application.Features.Products;

public class ProductBaseDto
{
    public bool IsPieceTracked { get; set; }
    public short ProductTypeId { get; set; } = 1;
    public bool IsSold { get; set; } = true;
    public bool IsPurchased { get; set; } = true;
    public string? Code { get; set; }
    public string? Sku { get; set; }
    public string? Article { get; set; }
    public int? ProductGroupId { get; set; }
    public short UnitId { get; set; }
    public string? Barcode { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsService { get; set; }
    public string? Mxik { get; set; }
    public short? DefaultVatRateId { get; set; }
    public decimal? MinStock { get; set; }
}
