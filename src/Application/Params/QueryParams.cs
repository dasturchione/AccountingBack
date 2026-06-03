using System.Linq.Expressions;

namespace Application.Params
{
    public class QueryParams<TEntity>
    {
        public Expression<Func<TEntity, bool>>? Criteria { get; set; }

        public Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? Sort { get; set; }
    }

    public class QueryParams<TEntity, TResult>
    {
        public Expression<Func<TEntity, bool>>? Criteria { get; set; }

        public Func<IQueryable<TResult>, IOrderedQueryable<TResult>>? Sort { get; set; }
    }
}
