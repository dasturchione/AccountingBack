namespace Application.Features.SaleDocs;

public class SaleDocBaseDto
{
    public DateTime DocDate { get; set; }
    public int CounterpartyId { get; set; }
    public int WarehouseId { get; set; }
    public short CurrencyId { get; set; }
    public decimal ExchangeRate { get; set; } = 1m;
    public long? ContractId { get; set; }
    public int? CustomerAccountId { get; set; }
    public int? VatAccountId { get; set; }
    public string? Comment { get; set; }

    /// <summary>Prices on this document already contain VAT, so it is extracted rather than added on top.</summary>
    public bool PriceIncludesVat { get; set; }
}
