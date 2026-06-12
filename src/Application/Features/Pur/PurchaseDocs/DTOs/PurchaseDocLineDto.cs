namespace Application.Features.PurchaseDocs;

public class PurchaseDocLineDto
{
    public int ProductTableId { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public short? VatRateId { get; set; }
}
