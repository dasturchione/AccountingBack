using System.Linq.Expressions;

namespace Application.Options
{
    public class QueryOptions<TEntity> where TEntity : class
    {
        public Expression<Func<TEntity, bool>>? Filter { get; init; }

        public Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? OrderBy { get; init; }

        public List<Expression<Func<TEntity, object>>> Includes { get; init; } = [];
    }

    public class QueryOptions<TEntity, TResult> where TEntity : class
    {
        public Expression<Func<TEntity, bool>>? Filter { get; init; }

        public Func<IQueryable<TResult>, IOrderedQueryable<TResult>>? OrderBy { get; init; }

        public Expression<Func<TEntity, TResult>> Select { get; init; } = null!;
    }
}
