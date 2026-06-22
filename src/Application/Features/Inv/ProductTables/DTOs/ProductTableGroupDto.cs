namespace Application.Features.ProductTables;

public class ProductTableGroupSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal TotalAmount { get; set; }
}

public class ProductTableProductSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Barcode { get; set; }
    public string? ProductGroupName { get; set; }
    public string UnitName { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal TotalAmount => Price * Quantity;
}

public class ProductTableItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public string? SerialNumber { get; set; }
    public string? MarkingNumber { get; set; }
}
