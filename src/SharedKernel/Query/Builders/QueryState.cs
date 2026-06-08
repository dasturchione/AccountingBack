using System.Linq.Expressions;

namespace SharedKernel.Query.Builders
{
    public class QueryState<TEntity> where TEntity : class
    {
        public IQueryBuilderResolver Resolver { get; init; } = null!;
        internal Expression<Func<TEntity, bool>> Criteria { get; set; } = _ => true;
        internal object? ResultCriteria { get; set; }
        internal object? OrderKey { get; set; }
        internal bool OrderDescending { get; set; }
        internal Type? ResultType { get; set; }
        internal object? Selector { get; set; }
    }
}
