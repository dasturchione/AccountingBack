namespace Application.Features.ProductGroups;

public class ProductGroupBaseDto
{
    public int? ParentId { get; set; }
    public string Name { get; set; } = null!;
}

public class ProductInGroupBaseDto
{
    public short UnitId { get; set; }
    public string? Barcode { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsService { get; set; }
}
