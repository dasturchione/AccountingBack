namespace Application.Features.ProductGroups;

public class ProductGroupCreateDto : ProductGroupBaseDto
{
    public List<ProductInGroupDto> Products { get; set; } = [];
}

public class ProductInGroupDto
{
    public short UnitId { get; set; }
    public string? Barcode { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsService { get; set; }
}
