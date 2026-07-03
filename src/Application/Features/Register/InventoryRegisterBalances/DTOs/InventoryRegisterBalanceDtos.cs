namespace Application.Features.InventoryRegisterBalances;

public class InventoryRegisterBalanceBaseDto
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
    public long? PostingBatchId { get; set; }
    public long? SourceLineId { get; set; }
    public long? ReversalEntryId { get; set; }
}

public class InventoryRegisterBalanceCreateDto : InventoryRegisterBalanceBaseDto { }

public class InventoryRegisterBalanceUpdateDto : InventoryRegisterBalanceBaseDto { }

public class InventoryRegisterBalanceDto : InventoryRegisterBalanceBaseDto
{
    public long Id { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class InventoryRegisterBalanceListDto : InventoryRegisterBalanceDto { }
