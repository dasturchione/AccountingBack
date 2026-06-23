namespace Application.Features.PurchaseDocs;

public class PurchaseDocServiceLineDto
{
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
    public int AccountId { get; set; }
}
