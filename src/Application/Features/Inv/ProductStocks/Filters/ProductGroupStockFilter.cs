using SharedKernel.Filters;

namespace Application.Features.Inv.ProductStocks
{
    public class ProductGroupStockFilter : IPaginationFilter
    {
        

        public int Page { get; set; } = 1;

        public int? PageSize { get; set; }
    }
}
