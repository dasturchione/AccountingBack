namespace Application.Features.FaReceipts;

public class FaReceiptDto
{
    public long Id { get; set; }
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = null!;
    public string DocNumber { get; set; } = null!;
    public DateTime DocDate { get; set; }
    public int? CounterpartyId { get; set; }
    public string? CounterpartyName { get; set; }
    public short CurrencyId { get; set; }
    public string CurrencyName { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public decimal VatAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public short StatusId { get; set; }
    public string StatusName { get; set; } = null!;
    public short ReceiptTypeId { get; set; }
    public string ReceiptTypeCode { get; set; } = null!;
    public string ReceiptTypeName { get; set; } = null!;
    public int? SupplierAccountId { get; set; }
    public string? SupplierAccountNumber { get; set; }
    public string? SupplierAccountName { get; set; }

    /// <summary>Prices on this document already contain VAT, so it is extracted rather than added on top.</summary>
    public bool PriceIncludesVat { get; set; }
    public short StateId { get; set; }
    public string StateName { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
    public DateTime? PostedAt { get; set; }
    public int? PostedByUserId { get; set; }
    public DateTime? CancelledAt { get; set; }
    public int? CancelledByUserId { get; set; }
    public List<FaReceiptLineDto> Lines { get; set; } = new();
}

public class FaReceiptLineDto
{
    public long Id { get; set; }
    public long ReceiptDocId { get; set; }
    public string Name { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Amount { get; set; }
    public short? VatRateId { get; set; }
    public string? VatRateName { get; set; }
    public decimal VatAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public int? CapitalInvestmentAccountId { get; set; }
    public string? CapitalInvestmentAccountNumber { get; set; }
    public string? CapitalInvestmentAccountName { get; set; }
    public int? VatAccountId { get; set; }
    public string? VatAccountNumber { get; set; }
    public string? VatAccountName { get; set; }
    public List<FaReceiptAssetDto> Assets { get; set; } = new();
}

public class FaReceiptAssetDto
{
    public long Id { get; set; }
    public long ReceiptDocLineId { get; set; }
    public long? FaAssetId { get; set; }
    public string InventoryNumber { get; set; } = null!;
    public string Name { get; set; } = null!;
    public decimal InitialCost { get; set; }
    public int FaGroupId { get; set; }
    public string FaGroupCode { get; set; } = null!;
    public string FaGroupName { get; set; } = null!;
    public short? OkofId { get; set; }
    public string? OkofCode { get; set; }
    public string? OkofName { get; set; }
    public int AssetAccountId { get; set; }
    public string AssetAccountNumber { get; set; } = null!;
    public string AssetAccountName { get; set; } = null!;
}
