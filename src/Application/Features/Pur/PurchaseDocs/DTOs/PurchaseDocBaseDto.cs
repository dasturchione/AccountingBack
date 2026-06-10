namespace Application.Features.PurchaseDocs;

public class PurchaseDocBaseDto
{
    public int OrganizationId { get; set; }
    public string DocNumber { get; set; } = null!;
    public DateTime DocDate { get; set; }
    public int CounterpartyId { get; set; }
    public int WarehouseId { get; set; }
    public short CurrencyId { get; set; }
    public string? Comment { get; set; }
    public List<PurchaseDocLineDto> Lines { get; set; } = new();
}
