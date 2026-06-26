namespace Application.Features.PurchaseDocTables;

public class PurchaseDocTableBaseDto
{
    public long OwnerId { get; set; }
    public short ItemTypeId { get; set; }
    public int? ProductTableId { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public short? VatRateId { get; set; }
}
