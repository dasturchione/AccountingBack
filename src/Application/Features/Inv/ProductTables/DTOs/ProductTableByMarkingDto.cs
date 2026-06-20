namespace Application.Features.ProductTables;

public class ProductTableByMarkingDto
{
    public int ProductTableId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public string? SerialNumber { get; set; }
    public string? MarkingNumber { get; set; }
}
