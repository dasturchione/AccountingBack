namespace Application.Features.RetailSaleDocs;

public class RetailSaleDocCreateDto
{
    public DateTime? DocDate { get; set; }
    public int? CounterpartyId { get; set; }
    public int WarehouseId { get; set; }
    public int CashRegisterId { get; set; }
    public short CurrencyId { get; set; }
    public decimal ExchangeRate { get; set; } = 1m;
    public int? ReceivableAccountId { get; set; }
    public int? VatAccountId { get; set; }
    public string? Comment { get; set; }

    /// <summary>Prices on this document already contain VAT, so it is extracted rather than added on top.</summary>
    public bool PriceIncludesVat { get; set; }
    public RetailSaleProcessingMode ProcessingMode { get; set; } = RetailSaleProcessingMode.Immediate;
    public List<RetailSaleDocProductCreateDto> Lines { get; set; } = new();
    public List<RetailSaleDocPaymentDto> Payments { get; set; } = new();
}

public class RetailSaleDocUpdateDto
{
    public DateTime DocDate { get; set; }
    public int? CounterpartyId { get; set; }
    public int WarehouseId { get; set; }
    public int CashRegisterId { get; set; }
    public short CurrencyId { get; set; }
    public decimal ExchangeRate { get; set; } = 1m;
    public int? ReceivableAccountId { get; set; }
    public int? VatAccountId { get; set; }
    public string? Comment { get; set; }

    /// <summary>Prices on this document already contain VAT, so it is extracted rather than added on top.</summary>
    public bool PriceIncludesVat { get; set; }
    public short StateId { get; set; }
    public List<RetailSaleDocProductCreateDto> Lines { get; set; } = new();
    public List<RetailSaleDocPaymentDto> Payments { get; set; } = new();
}

public class RetailSaleDocProductCreateDto
{
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public short UnitId { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal CostPrice { get; set; }
    public short? VatRateId { get; set; }
    /// <summary>Total VAT amount for the entire line; when null, it is calculated from VatRateId.</summary>
    public decimal? VatAmount { get; set; }
    public int? InventoryAccountId { get; set; }
    public int? IncomeAccountId { get; set; }
    public int? CostAccountId { get; set; }
    public List<RetailSaleDocProductTableCreateDto> Items { get; set; } = new();
}

public class RetailSaleDocProductTableCreateDto
{
    public int ProductTableId { get; set; }
}

public class RetailSaleDocPaymentDto
{
    public short PaymentMethodId { get; set; }
    public int? PaymentAcceptancePointId { get; set; }
    public int DebitAccountId { get; set; }
    public decimal Amount { get; set; }
    public string? TransactionNumber { get; set; }
}

public class RetailSaleDocConfirmDto
{
    public List<RetailSaleDocConfirmLineDto> Lines { get; set; } = new();
    public List<RetailSaleDocPaymentDto>? Payments { get; set; }
}

public class RetailSaleDocConfirmLineDto
{
    public long Id { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal CostPrice { get; set; }
    /// <summary>Total VAT amount for the entire line; when null, it is calculated from the line VatRateId.</summary>
    public decimal? VatAmount { get; set; }
}

public enum RetailSaleProcessingMode
{
    Draft = 1,
    Immediate = 2
}

public class RetailSaleDocDto
{
    public long Id { get; set; }
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = null!;
    public string DocNumber { get; set; } = null!;
    public DateTime DocDate { get; set; }
    public int? CounterpartyId { get; set; }
    public string? CounterpartyName { get; set; }
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = null!;
    public int CashRegisterId { get; set; }
    public string CashRegisterName { get; set; } = null!;
    public short CurrencyId { get; set; }
    public string CurrencyName { get; set; } = null!;
    public string CurrencyCode { get; set; } = null!;
    public decimal ExchangeRate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal VatAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public int? ReceivableAccountId { get; set; }
    public string? ReceivableAccountNumber { get; set; }
    public string? ReceivableAccountName { get; set; }
    public int? VatAccountId { get; set; }
    public string? VatAccountNumber { get; set; }
    public string? VatAccountName { get; set; }
    public short StatusId { get; set; }
    public string StatusName { get; set; } = null!;
    public short StateId { get; set; }
    public string StateName { get; set; } = null!;
    public string? Comment { get; set; }

    /// <summary>Prices on this document already contain VAT, so it is extracted rather than added on top.</summary>
    public bool PriceIncludesVat { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? PostedAt { get; set; }
    public int? PostedByUserId { get; set; }
    public DateTime? CancelledAt { get; set; }
    public int? CancelledByUserId { get; set; }
    public List<RetailSaleDocProductDto> Lines { get; set; } = new();
    public List<RetailSaleDocPaymentReadDto> Payments { get; set; } = new();
}

public class RetailSaleDocProductDto
{
    public long Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public string? ProductMxik { get; set; }
    public bool IsService { get; set; }
    public bool IsPieceTracked { get; set; }
    public decimal Quantity { get; set; }
    public short UnitId { get; set; }
    public string UnitName { get; set; } = null!;
    public decimal UnitPrice { get; set; }
    public decimal CostPrice { get; set; }
    public decimal Amount { get; set; }
    public short? VatRateId { get; set; }
    public string? VatRateName { get; set; }
    public decimal VatAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public int? InventoryAccountId { get; set; }
    public int? IncomeAccountId { get; set; }
    public int? CostAccountId { get; set; }
    public List<RetailSaleDocProductTableDto> Items { get; set; } = new();
}

public class RetailSaleDocProductTableDto
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

public class RetailSaleDocPaymentReadDto
{
    public long Id { get; set; }
    public short PaymentMethodId { get; set; }
    public string PaymentMethodName { get; set; } = null!;
    public int? PaymentAcceptancePointId { get; set; }
    public string? PaymentAcceptancePointName { get; set; }
    public int DebitAccountId { get; set; }
    public string DebitAccountNumber { get; set; } = null!;
    public string DebitAccountName { get; set; } = null!;
    public decimal Amount { get; set; }
    public string? TransactionNumber { get; set; }
}

public class RetailSaleDocListDto
{
    public long Id { get; set; }
    public string DocNumber { get; set; } = null!;
    public DateTime DocDate { get; set; }
    public int? CounterpartyId { get; set; }
    public string? CounterpartyName { get; set; }
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = null!;
    public int CashRegisterId { get; set; }
    public string CashRegisterName { get; set; } = null!;
    public short CurrencyId { get; set; }
    public string CurrencyCode { get; set; } = null!;
    public decimal FinalAmount { get; set; }
    public short StatusId { get; set; }
    public string StatusName { get; set; } = null!;
    public short StateId { get; set; }
    public string StateName { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
}
