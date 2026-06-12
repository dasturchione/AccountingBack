namespace Application.Features.PurchaseDocs;

public class PurchaseDocLineDto
{
    public int ProductId { get; set; }
    public string MarkingNumber { get; set; } = null!;
    public string? SerialNumber { get; set; } 
    public decimal Price { get; set; }
    public short? VatRateId { get; set; }
}
