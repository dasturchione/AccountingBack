namespace Application.Features.WarehouseTransfers;

public class WarehouseTransferPostingBatchDto
{
    public long Id { get; set; }
    public int OrganizationId { get; set; }
    public short DocumentTypeId { get; set; }
    public long DocumentId { get; set; }
    public string Status { get; set; } = null!;
    public int? PostedByUserId { get; set; }
    public DateTime PostedAt { get; set; }
    public int? ReversedByUserId { get; set; }
    public DateTime? ReversedAt { get; set; }
    public string? Comment { get; set; }
}
