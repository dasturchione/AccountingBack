using SharedKernel.Query.Specifications;
using System.Linq.Expressions;

namespace SharedKernel.Query.Builders
{
    public class ResultQueryBuilder<TEntity, TResult> where TEntity : class
    {
        internal readonly QueryState<TEntity> State;
        internal Expression<Func<TResult, bool>> ResultCriteria { get; set; } = _ => true;
        internal Expression<Func<TResult, object>>? OrderKey { get; set; }
        internal bool OrderDescending { get; set; }

        internal ResultQueryBuilder(QueryState<TEntity> state)
        {
            State = state;
        }

        public ResultQueryBuilder<TEntity, TResult> Where(Expression<Func<TResult, bool>> criteria)
        {
            ResultCriteria = criteria;
            return this;
        }

        public ResultOrderByBuilder<TEntity, TResult> OrderBy(
            Expression<Func<TResult, object>> keySelector)
        {
            return new ResultOrderByBuilder<TEntity, TResult>(this, keySelector);
        }

        public QuerySpecification<TEntity, TResult> Build()
        {
            var selector = State.Selector is Expression<Func<TEntity, TResult>> explicitSelector
                                    ? explicitSelector
                                    : State.Resolver.GetProjectionBuilder<TEntity, TResult>().Build();

            return new QuerySpecification<TEntity, TResult>
            {
                Criteria = State.Criteria,
                ResultCriteria = ResultCriteria,
                OrderBy = BuildOrderBy(),
                Selector = selector
            };
        }

        private Func<IQueryable<TResult>, IOrderedQueryable<TResult>>? BuildOrderBy()
        {
            if (OrderKey is null)
                return null;

            return OrderDescending
                ? q => q.OrderByDescending(OrderKey)
                : q => q.OrderBy(OrderKey);
        }
    }
}
