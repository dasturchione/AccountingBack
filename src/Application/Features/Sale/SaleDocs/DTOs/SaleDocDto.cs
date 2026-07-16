namespace Application.Features.SaleDocs;

public class SaleDocDto
{
    public long Id { get; set; }
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = null!;
    public string DocNumber { get; set; } = null!;
    public DateTime DocDate { get; set; }
    public int CounterpartyId { get; set; }
    public string CounterpartyName { get; set; } = null!;
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = null!;
    public short CurrencyId { get; set; }
    public string CurrencyName { get; set; } = null!;
    public string CurrencyCode { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public decimal VatAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public decimal ExchangeRate { get; set; }
    public int? CustomerAccountId { get; set; }
    public string? CustomerAccountNumber { get; set; }
    public string? CustomerAccountName { get; set; }
    public int? VatAccountId { get; set; }
    public string? VatAccountNumber { get; set; }
    public string? VatAccountName { get; set; }
    public DateTime? PostedAt { get; set; }
    public int? PostedByUserId { get; set; }
    public DateTime? CancelledAt { get; set; }
    public int? CancelledByUserId { get; set; }
    public short StatusId { get; set; }
    public string StatusName { get; set; } = null!;
    public string? Comment { get; set; }
    public short StateId { get; set; }
    public string StateName { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
    public long? ContractId { get; set; }
    public string? ContractNumber { get; set; }

    public List<SaleDocProductDto> Lines { get; set; } = new();
}

public class SaleDocProductDto
{
    public long Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public string? ProductMxik { get; set; }
    public bool IsService { get; set; }
    public decimal Quantity { get; set; }
    public short UnitId { get; set; }
    public string UnitName { get; set; } = null!;
    public decimal UnitPrice { get; set; }
    public decimal CostPrice { get; set; }
    public decimal Amount { get; set; }
    public short? VatRateId { get; set; }
    public string? VatRateName { get; set; }
    public int? InventoryAccountId { get; set; }
    public string? InventoryAccountNumber { get; set; }
    public string? InventoryAccountName { get; set; }
    public int? IncomeAccountId { get; set; }
    public string? IncomeAccountNumber { get; set; }
    public string? IncomeAccountName { get; set; }
    public int? CostAccountId { get; set; }
    public string? CostAccountNumber { get; set; }
    public string? CostAccountName { get; set; }
    public decimal VatAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public List<SaleDocProductTableDto> Items { get; set; } = new();
    public List<SaleDocProductBatchReadDto> Batches { get; set; } = new();
}

public class SaleDocProductBatchReadDto
{
    public long BatchId { get; set; }
    public decimal Quantity { get; set; }
}

public class SaleDocProductTableDto
{
    public long Id { get; set; }
    public int ProductTableId { get; set; }
    public string? MarkingNumber { get; set; }
    public string? SerialNumber { get; set; }
    public decimal CostPrice { get; set; }
    public decimal Amount { get; set; }
    public short? VatRateId { get; set; }
    public decimal VatAmount { get; set; }
    public decimal TotalAmount { get; set; }
}
