namespace Application.Features.Inv.ProductPrices;

public class ProductPriceBaseDto
{
    public int ProductId { get; set; }
    public short CurrencyId { get; set; }
    public short PriceTypeId { get; set; }
    public short UnitId { get; set; }
    public decimal Price { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
