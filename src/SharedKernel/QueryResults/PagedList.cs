namespace SharedKernel.QueryResults
{
    public sealed record PagedList<TItem>(List<TItem> Items, int TotalCount);
}
