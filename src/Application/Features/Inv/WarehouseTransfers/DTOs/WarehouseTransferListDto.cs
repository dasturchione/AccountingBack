namespace Application.Features.WarehouseTransfers;

public class WarehouseTransferListDto
{
    public long Id { get; set; }
    public int OrganizationId { get; set; }
    public string DocNumber { get; set; } = null!;
    public DateTime DocDate { get; set; }
    public int SourceWarehouseId { get; set; }
    public string SourceWarehouseName { get; set; } = null!;
    public int DestinationWarehouseId { get; set; }
    public string DestinationWarehouseName { get; set; } = null!;
    public short StatusId { get; set; }
    public string StatusName { get; set; } = null!;
    public short StateId { get; set; }
    public string StateName { get; set; } = null!;
    public string? Comment { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? PostedAt { get; set; }
    public int? PostedByUserId { get; set; }
    public DateTime? CancelledAt { get; set; }
    public int? CancelledByUserId { get; set; }
}
