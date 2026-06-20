namespace Application.Features.SaleDocs;

public class SaleDocLineDto
{
    public int ProductId { get; set; }
    public string MarkingNumber { get; set; } = null!;
    public string? SerialNumber { get; set; }
    public decimal Price { get; set; }
    public short? VatRateId { get; set; }
}
