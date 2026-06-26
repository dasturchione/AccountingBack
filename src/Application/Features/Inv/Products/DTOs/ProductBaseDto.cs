namespace Application.Features.Products;

public class ProductBaseDto
{
    public int? ProductGroupId { get; set; }
    public short UnitId { get; set; }
    public string? Barcode { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsService { get; set; }
    public string? Mxik { get; set; }
}
