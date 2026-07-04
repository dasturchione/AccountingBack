namespace Application.Features.Products;

public class ProductListDto
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = null!;
    public int? ProductGroupId { get; set; }
    public string? ProductGroupName { get; set; }
    public short ProductTypeId { get; set; }
    public string ProductTypeName { get; set; } = null!;
    public string ProductTypeCode { get; set; } = null!;
    public short UnitId { get; set; }
    public string UnitName { get; set; } = null!;
    public string? Code { get; set; }
    public string? Sku { get; set; }
    public string? Article { get; set; }
    public string? Barcode { get; set; }
    public string Name { get; set; } = null!;
    public bool IsPieceTracked { get; set; }
    public bool IsService { get; set; }
    public bool IsSold { get; set; }
    public bool IsPurchased { get; set; }
    public string? Mxik { get; set; }
    public short? DefaultVatRateId { get; set; }
    public decimal? MinStock { get; set; }
    public short StateId { get; set; }
    public string StateName { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
}
