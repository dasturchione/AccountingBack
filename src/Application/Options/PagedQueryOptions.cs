namespace Application.Options
{
    public sealed class PagedQueryOptions<TEntity> : QueryOptions<TEntity> where TEntity : class
    {
        public int Skip { get; init; }

        public int? Take { get; init; }
    }

    public sealed class PagedQueryOptions<TEntity, TResult> : QueryOptions<TEntity, TResult> where TEntity : class
    {
        public int Skip { get; init; }

        public int? Take { get; init; }
    }
}
