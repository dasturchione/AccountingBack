namespace Application.Common.Pagination;

public sealed class PagedResponse<T>
{
    public IReadOnlyCollection<T> Items { get; init; } = new List<T>();

    public int Page { get; init; }

    public int PageSize { get; init; }

    public int TotalCount { get; init; }

    public int TotalPages { get; init; }

    public bool HasPreviousPage { get; init; }

    public bool HasNextPage { get; init; }
}
