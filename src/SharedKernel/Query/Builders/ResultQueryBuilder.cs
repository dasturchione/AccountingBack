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
        internal Func<IQueryable<TResult>, IOrderedQueryable<TResult>>? CustomOrderBy { get; set; }

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

        public ResultQueryBuilder<TEntity, TResult> OrderBy(
            Func<IQueryable<TResult>, IOrderedQueryable<TResult>> orderBy)
        {
            CustomOrderBy = orderBy;
            return this;
        }

        public ResultQueryBuilder<TEntity, TResult> IgnoreQueryFilters()
        {
            State.IgnoreQueryFilters = true;
            return this;
        }

        public ResultQueryBuilder<TEntity, TResult> Skip(int skip)
        {
            State.Skip = skip;
            return this;
        }

        public ResultQueryBuilder<TEntity, TResult> Take(int? take)
        {
            State.Take = take;
            return this;
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
                Selector = selector,
                IgnoreQueryFilters = State.IgnoreQueryFilters
            };
        }

        public PagedQuerySpecification<TEntity, TResult> BuildPaged()
        {
            var selector = State.Selector is Expression<Func<TEntity, TResult>> explicitSelector
                ? explicitSelector
                : State.Resolver.GetProjectionBuilder<TEntity, TResult>().Build();

            return new PagedQuerySpecification<TEntity, TResult>
            {
                Criteria = State.Criteria,
                ResultCriteria = ResultCriteria,
                OrderBy = BuildOrderBy(),
                Selector = selector,
                IgnoreQueryFilters = State.IgnoreQueryFilters,
                Skip = State.Skip,
                Take = State.Take
            };
        }

        private Func<IQueryable<TResult>, IOrderedQueryable<TResult>>? BuildOrderBy()
        {
            if (CustomOrderBy is not null)
                return CustomOrderBy;

            if (OrderKey is null)
                return State.Resolver.GetOrderByBuilder<TEntity, TResult>()?.Build();

            return OrderDescending
                ? q => q.OrderByDescending(OrderKey)
                : q => q.OrderBy(OrderKey);
        }
    }
}
