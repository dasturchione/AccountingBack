namespace Application.Features.InventoryMovements;

public sealed class InventoryMovementEntry
{
    public int OrganizationId { get; set; }
    public short DocumentTypeId { get; set; }
    public long DocumentId { get; set; }
    public int WarehouseId { get; set; }
    public int ProductId { get; set; }
    public int? ProductTableId { get; set; }
    public short OperationTypeId { get; set; }
    public decimal Quantity { get; set; }
    public decimal Amount { get; set; }
    public DateTime DocDate { get; set; }
    public long? SourceLineId { get; set; }
    public long? OriginalMovementId { get; set; }
}
