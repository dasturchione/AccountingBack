namespace Application.Features.SaleDocs;

public class SaleDocUpdateDto : SaleDocBaseDto
{
    public short StateId { get; set; }
    public List<SaleDocUpdateProductDto> Products { get; set; } = new();
}

public class SaleDocUpdateProductDto
{
    public long? Id { get; set; }
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public short? VatRateId { get; set; }
}
