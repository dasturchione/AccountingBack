namespace Application.Features.SaleShipments;

public sealed class SaleShipmentListDto
{
    public long Id { get; set; }
    public long? SaleDocId { get; set; }
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = null!;
    public int? CounterpartyId { get; set; }
    public string? CounterpartyName { get; set; }
    public string? DocNumber { get; set; }
    public DateTime DocDate { get; set; }
    public short StatusId { get; set; }
    public string StatusName { get; set; } = null!;
    public string? Comment { get; set; }
    public DateTime CreatedDate { get; set; }
}
