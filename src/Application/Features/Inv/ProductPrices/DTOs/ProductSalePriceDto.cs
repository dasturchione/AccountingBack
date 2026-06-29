namespace Application.Features.Inv.ProductPrices
{
    public class ProductSalePriceDto
    {
        public decimal SalePrice { get; set; }
        public List<ProductSalePriceTableDto> SalePrices { get; set; } = new();
    }

    public class ProductSalePriceTableDto
    {
        public long PurchaseId { get; set; }
        public DateTime PurchaseDate { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Quantity { get => ProductTableIds.Count(); }
        public List<int> ProductTableIds { get; set; } = new();
    }
}
