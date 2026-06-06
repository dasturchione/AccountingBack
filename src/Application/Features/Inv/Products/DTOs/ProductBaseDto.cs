namespace Application.Features.Products;

public class ProductBaseDto
{
    public int OrganizationId { get; set; }
    public int? ProductGroupId { get; set; }
    public short UnitId { get; set; }
    public string Code { get; set; } = null!;
    public string? Barcode { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsService { get; set; }
}
