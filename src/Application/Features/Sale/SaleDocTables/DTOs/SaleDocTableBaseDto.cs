namespace Application.Features.SaleDocTables;

public class SaleDocTableBaseDto
{
    public long OwnerId { get; set; }
    public int ProductTableId { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public short? VatRateId { get; set; }
}
