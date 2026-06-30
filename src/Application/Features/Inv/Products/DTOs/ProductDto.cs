namespace Application.Features.Products;

public class ProductDto
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = null!;
    public int? ProductGroupId { get; set; }
    public string? ProductGroupName { get; set; }
    public short UnitId { get; set; }
    public string UnitName { get; set; } = null!;
    public string? Code { get; set; }
    public string? Sku { get; set; }
    public string? Article { get; set; }
    public string? Barcode { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsPieceTracked { get; set; }
    public bool IsService { get; set; }
    public string? Mxik { get; set; }
    public short? DefaultVatRateId { get; set; }
    public int? InventoryAccountId { get; set; }
    public int? IncomeAccountId { get; set; }
    public int? ExpenseAccountId { get; set; }
    public int? CogsAccountId { get; set; }
    public decimal? MinStock { get; set; }
    public short StateId { get; set; }
    public string StateName { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
}
