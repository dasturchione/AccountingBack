namespace Application.Features.Inv.ProductStocks
{
    public class ProductTableStockDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string? Mxik { get; set; }
        public string? SerialNumber { get; set; }
        public string? MarkingNumber { get; set; }
    }
}
