namespace Application.Features.Products;

public class ProductBaseDto
{
    public bool IsPieceTracked { get; set; }
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
    public int? InventoryAccountId { get; set; }
    public int? IncomeAccountId { get; set; }
    public int? ExpenseAccountId { get; set; }
    public int? CogsAccountId { get; set; }
    public decimal? MinStock { get; set; }
}
