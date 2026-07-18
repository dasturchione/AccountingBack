namespace Application.Features.SaleShipments;

public sealed class SaleShipmentDto
{
    public long Id { get; set; }
    public long? SaleDocId { get; set; }
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = null!;
    public int? CounterpartyId { get; set; }
    public string? CounterpartyName { get; set; }
    public string? DocNumber { get; set; }
    public DateTime DocDate { get; set; }
    public string? Comment { get; set; }
    public short StatusId { get; set; }
    public string StatusName { get; set; } = null!;
    public IReadOnlyList<SaleShipmentProductDto> Products { get; set; } = [];
}

public sealed class SaleShipmentProductDto
{
    public long Id { get; set; }
    public long? SaleDocProductId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public short UnitId { get; set; }
    public string UnitName { get; set; } = null!;
    public decimal Quantity { get; set; }
    public IReadOnlyList<SaleShipmentProductBatchDto> Batches { get; set; } = [];
    public IReadOnlyList<SaleShipmentTableDto> ProductTables { get; set; } = [];
}

public sealed class SaleShipmentProductBatchDto
{
    public long BatchId { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime ReceivedDate { get; set; }
    public decimal Quantity { get; set; }
}

public sealed class SaleShipmentTableDto
{
    public long Id { get; set; }
    public int ProductTableId { get; set; }
    public string? MarkingNumber { get; set; }
    public string? SerialNumber { get; set; }
}
