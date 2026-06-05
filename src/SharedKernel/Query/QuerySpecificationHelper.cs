using SharedKernel.Query.Specifications;
using System.Linq.Expressions;

namespace SharedKernel.Query
{
    public static class Query
    {
        public static QuerySpecification<TEntity> Where<TEntity>(
        Expression<Func<TEntity, bool>> criteria)
        where TEntity : class =>
        new() { Criteria = criteria };

        public static QuerySpecification<TEntity> Where<TEntity>(
            Expression<Func<TEntity, bool>> criteria,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy)
            where TEntity : class =>
            new() { Criteria = criteria, OrderBy = orderBy };
    }
}
