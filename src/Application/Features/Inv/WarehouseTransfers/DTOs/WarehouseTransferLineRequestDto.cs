namespace Application.Features.WarehouseTransfers;

public class WarehouseTransferLineRequestDto
{
    public int ProductId { get; set; }
    public short UnitId { get; set; }
    public decimal Quantity { get; set; }
    public string? Comment { get; set; }
    public List<WarehouseTransferTableRequestDto> Items { get; set; } = new();
}

public class WarehouseTransferTableRequestDto
{
    public int ProductTableId { get; set; }
    public decimal CostPrice { get; set; }
}
