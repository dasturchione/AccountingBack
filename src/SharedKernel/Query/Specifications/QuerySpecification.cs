using SharedKernel.Query.Includes;
using System.Linq.Expressions;

namespace SharedKernel.Query.Specifications
{
    public class QuerySpecification<TEntity> where TEntity : class
    {
        public Expression<Func<TEntity, bool>> Criteria { get; init; } = _ => true;
        public Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? OrderBy { get; init; }
        public bool IgnoreQueryFilters { get; init; }

        public IReadOnlyList<IncludeEntry<TEntity>> Includes => _includes;

        private readonly List<IncludeEntry<TEntity>> _includes = [];

        public void AddIncludes(Action<IncludeBuilder<TEntity>> builder)
        {
            var includeBuilder = new IncludeBuilder<TEntity>();
            builder(includeBuilder);
            _includes.AddRange(includeBuilder.Entries);
        }
    }

    public class QuerySpecification<TEntity, TResult> where TEntity : class
    {
        public Expression<Func<TEntity, bool>> Criteria { get; init; } = _ => true;
        public Expression<Func<TResult, bool>> ResultCriteria { get; init; } = _ => true;
        public Func<IQueryable<TResult>, IOrderedQueryable<TResult>>? OrderBy { get; init; }
        public Expression<Func<TEntity, TResult>> Selector { get; init; } = null!;
        public bool IgnoreQueryFilters { get; init; }
    }
}
