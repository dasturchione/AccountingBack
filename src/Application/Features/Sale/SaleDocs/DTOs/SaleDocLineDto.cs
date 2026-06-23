namespace Application.Features.SaleDocs;

public class SaleDocLineDto
{
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public short? VatRateId { get; set; }
}
