namespace Application.Features.SaleShipments;

public sealed class SaleShipmentCreateDto
{
    public int WarehouseId { get; set; }
    public int? CounterpartyId { get; set; }
    public DateTime DocDate { get; set; }
    public string? Comment { get; set; }
    public IReadOnlyList<SaleShipmentProductCreateDto> Products { get; set; } = [];
}

public sealed class SaleShipmentProductCreateDto
{
    public int ProductId { get; set; }
    public short UnitId { get; set; }
    public decimal Quantity { get; set; }
    public IReadOnlyList<SaleShipmentProductBatchCreateDto> Batches { get; set; } = [];
    public IReadOnlyList<SaleShipmentTableCreateDto> ProductTables { get; set; } = [];
}

public sealed class SaleShipmentProductBatchCreateDto
{
    public long BatchId { get; set; }
    public decimal Quantity { get; set; }
}

public sealed class SaleShipmentTableCreateDto
{
    public int ProductTableId { get; set; }
}
