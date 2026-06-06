using SharedKernel.QueryResults;

namespace Application.Common.Pagination;

public static class PagedResponseFactory
{
    public static PagedResponse<T> Create<T>(PagedList<T> data, int? page, int? pageSize)
    {
        var pageNumber = page ?? 1;
        var pageSizeNumber = pageSize ?? 50;

        var totalPages = data.TotalCount > 0
                ? (int)Math.Ceiling(data.TotalCount / (double)pageSizeNumber)
                : 0;

        return new PagedResponse<T>
        {
            Items = data.Items,
            TotalCount = data.TotalCount,
            Page = pageNumber,
            PageSize = pageSizeNumber,
            TotalPages = totalPages,
            HasPreviousPage = pageNumber > 1,
            HasNextPage = pageNumber < totalPages
        };
    }
}
