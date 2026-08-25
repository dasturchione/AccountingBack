using Application.Abstractions.Integration.Edo;

namespace Application.Features.PurchaseDocs;

public class PurchaseDocDto
{
    public long Id { get; set; }
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = null!;
    public string DocNumber { get; set; } = null!;
    public string? ExternalId { get; set; }
    public string? ExternalDocNumber { get; set; }
    public DateTime DocDate { get; set; }
    public int CounterpartyId { get; set; }
    public string CounterpartyName { get; set; } = null!;
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = null!;
    public short CurrencyId { get; set; }
    public string CurrencyName { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public decimal VatAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public decimal ExchangeRate { get; set; }
    public int? SupplierAccountId { get; set; }
    public string? SupplierAccountNumber { get; set; }
    public string? SupplierAccountName { get; set; }
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
    public EdoSourceMetadataDto? EdoSource { get; set; }

    public List<PurchaseDocProductDto> Lines { get; set; } = new();
}

public class PurchaseDocProductDto
{
    public long Id { get; set; }
    public long OwnerId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public string? ProductMxik { get; set; }
    public decimal Quantity { get; set; }
    public short UnitId { get; set; }
    public string UnitName { get; set; } = null!;
    public decimal UnitPrice { get; set; }
    public decimal Amount { get; set; }
    public short? VatRateId { get; set; }
    public string? VatRateName { get; set; }
    public int? DebitAccountId { get; set; }
    public string? DebitAccountNumber { get; set; }
    public string? DebitAccountName { get; set; }
    public int? VatAccountId { get; set; }
    public string? VatAccountNumber { get; set; }
    public string? VatAccountName { get; set; }
    public decimal VatAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public List<PurchaseDocProductItemDto> Items { get; set; } = new();
}

public class PurchaseDocProductItemDto
{
    public long Id { get; set; }
    public int ProductTableId { get; set; }
    public string? MarkingNumber { get; set; }
    public string? SerialNumber { get; set; }
    public bool HasMarking { get; set; }
    public int MarkingCount { get; set; }
    public string? VerificationState { get; set; }
    public string? SourceType { get; set; }
    public decimal Amount { get; set; }
    public short? VatRateId { get; set; }
    public string? VatRateName { get; set; }
    public decimal VatAmount { get; set; }
    public decimal TotalAmount { get; set; }
}
