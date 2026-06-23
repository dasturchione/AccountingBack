namespace Application.Features.SaleDocTables;

public class SaleDocTableBaseDto
{
    public long OwnerId { get; set; }
    public int ProductTableId { get; set; }
    public decimal Amount { get; set; }
    public short? VatRateId { get; set; }
}
