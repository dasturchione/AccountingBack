namespace Application.Features.WarehouseTransfers;

public class WarehouseTransferBaseDto
{
    public DateTime DocDate { get; set; }
    public int SourceWarehouseId { get; set; }
    public int DestinationWarehouseId { get; set; }
    public string? Comment { get; set; }
    public List<WarehouseTransferLineRequestDto> Lines { get; set; } = new();
}
