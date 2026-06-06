namespace Application.Features.PurchaseDocTables;

public class PurchaseDocTableBaseDto
{
    public long OwnerId { get; set; }
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public short? VatRateId { get; set; }
}
