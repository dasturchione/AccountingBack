namespace Application.Features.PurchaseDocs;

public class PurchaseDocBaseDto
{
    public string? ExternalId { get; set; }
    public string? ExternalDocNumber { get; set; }
    public DateTime DocDate { get; set; }
    public int CounterpartyId { get; set; }
    public int WarehouseId { get; set; }
    public short CurrencyId { get; set; }
    public decimal ExchangeRate { get; set; } = 1m;
    public long? ContractId { get; set; }
    public int? SupplierAccountId { get; set; }
    public string? Comment { get; set; }
}
