namespace Application.Features.PurchaseDocs;

public class PurchaseDocLineDto
{
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public short UnitId { get; set; }
    public decimal UnitPrice { get; set; }
    public short? VatRateId { get; set; }
    public int? DebitAccountId { get; set; }
    public int? VatAccountId { get; set; }
    public List<PurchaseDocLineItemDto> Items { get; set; } = new();
}

public class PurchaseDocLineItemDto
{
    public string MarkingNumber { get; set; } = null!;
    public string? SerialNumber { get; set; }
}
