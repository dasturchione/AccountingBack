namespace Application.Features.PurchaseDocs;

public class PurchaseDocBaseDto
{
    public DateTime DocDate { get; set; }
    public int CounterpartyId { get; set; }
    public int WarehouseId { get; set; }
    public short CurrencyId { get; set; }
    public decimal ExchangeRate { get; set; } = 1m;
    public string? Comment { get; set; }
    public List<PurchaseDocLineDto> Lines { get; set; } = new();
}
