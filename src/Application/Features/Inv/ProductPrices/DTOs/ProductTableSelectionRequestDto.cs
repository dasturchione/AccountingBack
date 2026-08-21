namespace Application.Features.Inv.ProductPrices;

public class ProductTableSelectionRequestDto
{
    public long LineId { get; set; }
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public bool IsPieceTracked { get; set; }
    public IReadOnlyCollection<int> ProductTableIds { get; set; } = Array.Empty<int>();
}
