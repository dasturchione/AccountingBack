namespace Application.Features.Inv.ProductPrices;

public class ProductTableSelectionDto
{
    public long LineId { get; set; }
    public int ProductId { get; set; }
    public int ProductTableId { get; set; }
    public decimal CostPrice { get; set; }
}
