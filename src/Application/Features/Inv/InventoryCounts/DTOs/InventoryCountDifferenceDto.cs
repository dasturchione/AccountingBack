namespace Application.Features.InventoryCounts;

public class InventoryCountDifferenceDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public short UnitId { get; set; }
    public string UnitName { get; set; } = null!;
    public decimal ExpectedQuantity { get; set; }
    public decimal CountedQuantity { get; set; }
    public decimal CorrectQuantity { get; set; }
    public decimal MissingQuantity { get; set; }
    public decimal FoundQuantity { get; set; }
    public List<int> MissingProductTableIds { get; set; } = new();
    public List<InventoryCountFoundItemDto> FoundItems { get; set; } = new();
}

public class InventoryCountFoundItemDto
{
    public int? ProductTableId { get; set; }
    public string? Barcode { get; set; }
    public string? SerialNumber { get; set; }
    public string? MarkingNumber { get; set; }
    public decimal CostPrice { get; set; }
}
