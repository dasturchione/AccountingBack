namespace Application.Features.ProductPrices;

public class ProductPriceBaseDto
{
    public int ProductId { get; set; }
    public short CurrencyId { get; set; }
    public decimal Price { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
