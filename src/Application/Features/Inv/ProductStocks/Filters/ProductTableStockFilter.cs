using SharedKernel.Filters;

namespace Application.Features.Inv.ProductStocks
{
    public class ProductTableStockFilter : IPaginationFilter
    {
        public int? ProductGroupId { get; set; }

        public int? ProductId { get; set; }

        public int Page { get; set; } = 1;

        public int? PageSize { get; set; }
    }
}
