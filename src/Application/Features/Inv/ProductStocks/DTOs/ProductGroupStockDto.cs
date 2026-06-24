namespace Application.Features.Inv.ProductStocks
{
    public class ProductGroupStockDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal CostPrice { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
