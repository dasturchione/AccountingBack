namespace Application.Features.PurchaseDocs;

public class PurchaseDocBaseDto
{
    public DateTime DocDate { get; set; }
    public int CounterpartyId { get; set; }
    public int WarehouseId { get; set; }
    public short CurrencyId { get; set; }
    public string? Comment { get; set; }
    public List<PurchaseDocLineDto> Lines { get; set; } = new();
    public List<PurchaseDocServiceLineDto> ServiceLines { get; set; } = new();
}
