namespace Application.Features.PurchaseDocs;

public class PurchaseDocLineDto
{
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public short? VatRateId { get; set; }
}
