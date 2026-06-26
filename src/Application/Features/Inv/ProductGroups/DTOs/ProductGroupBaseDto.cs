namespace Application.Features.ProductGroups;

public class ProductGroupBaseDto
{
    public string Name { get; set; } = null!;
}

public class ProductInGroupBaseDto
{
    public short UnitId { get; set; }
    public string? Barcode { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsPieceTracked { get; set; }
    public bool IsService { get; set; }
}
