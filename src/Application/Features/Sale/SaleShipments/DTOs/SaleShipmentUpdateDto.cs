namespace Application.Features.SaleShipments;

public sealed class SaleShipmentUpdateDto
{
    public int WarehouseId { get; set; }
    public int? CounterpartyId { get; set; }
    public string? DocNumber { get; set; }
    public DateTime DocDate { get; set; }
    public string? Comment { get; set; }
    public IReadOnlyList<SaleShipmentProductCreateDto> Products { get; set; } = [];
}
