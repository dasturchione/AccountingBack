namespace Application.Features.Products;

public class ProductListDto
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = null!;
    public int? ProductGroupId { get; set; }
    public string? ProductGroupName { get; set; }
    public short UnitId { get; set; }
    public string UnitName { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string? Barcode { get; set; }
    public string Name { get; set; } = null!;
    public bool IsService { get; set; }
    public short StateId { get; set; }
    public string StateName { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
}
