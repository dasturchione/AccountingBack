namespace Application.Features.FaReceipts;

public class FaReceiptBaseDto
{
    public DateTime DocDate { get; set; }
    public int? CounterpartyId { get; set; }
    public short CurrencyId { get; set; }
    public short ReceiptTypeId { get; set; }
    public int SupplierAccountId { get; set; }

    /// <summary>Prices on this document already contain VAT, so it is extracted rather than added on top.</summary>
    public bool PriceIncludesVat { get; set; }
    public List<FaReceiptLineWriteDto> Lines { get; set; } = new();
}

public class FaReceiptLineWriteDto
{
    public string Name { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public short? VatRateId { get; set; }
    public int CapitalInvestmentAccountId { get; set; }
    public int? VatAccountId { get; set; }
    public List<FaReceiptAssetWriteDto> Assets { get; set; } = new();
}

public class FaReceiptAssetWriteDto
{
    public string InventoryNumber { get; set; } = null!;
    public string Name { get; set; } = null!;
    public int FaGroupId { get; set; }
    public short? OkofId { get; set; }
    public decimal InitialCost { get; set; }
    public int AssetAccountId { get; set; }
}
