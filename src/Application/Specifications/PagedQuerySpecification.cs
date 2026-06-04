namespace Application.Specifications
{
    public class PagedQuerySpecification<TEntity> : QuerySpecification<TEntity> where TEntity : class
    {
        public int? Take { get; init; }
        public int Skip { get; init; } = 0;
    }

    public class PagedQuerySpecification<TEntity, TResult> : QuerySpecification<TEntity, TResult> where TEntity : class
    {
        public int? Take { get; init; }
        public int Skip { get; init; } = 0;
    }
}
