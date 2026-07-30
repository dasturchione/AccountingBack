namespace Application.Features.Inv.OpeningInventories;

public sealed class OpeningInventoryDto
{
    public long Id { get; set; }
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = null!;
    public string DocNumber { get; set; } = null!;
    public DateTime DocDate { get; set; }
    public int CounterpartyId { get; set; }
    public string CounterpartyName { get; set; } = null!;
    public long? ContractId { get; set; }
    public string? ContractNumber { get; set; }
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public short StatusId { get; set; }
    public string StatusName { get; set; } = null!;
    public string? Comment { get; set; }
    public short StateId { get; set; }
    public string StateName { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
    public DateTime? PostedAt { get; set; }
    public int? PostedByUserId { get; set; }
    public List<OpeningInventoryProductDto> Lines { get; set; } = [];
}

public sealed class OpeningInventoryProductDto
{
    public long Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public string? ProductMxik { get; set; }
    public decimal Quantity { get; set; }
    public short UnitId { get; set; }
    public string UnitName { get; set; } = null!;
    public decimal UnitPrice { get; set; }
    public decimal Amount { get; set; }
    public int DebitAccountId { get; set; }
    public string DebitAccountNumber { get; set; } = null!;
    public string DebitAccountName { get; set; } = null!;
    public long? WarehouseBatchId { get; set; }
    public List<OpeningInventoryItemDto> Items { get; set; } = [];
}

public sealed class OpeningInventoryItemDto
{
    public long Id { get; set; }
    public int ProductTableId { get; set; }
    public string? MarkingNumber { get; set; }
    public string? SerialNumber { get; set; }
    public decimal Amount { get; set; }
}
