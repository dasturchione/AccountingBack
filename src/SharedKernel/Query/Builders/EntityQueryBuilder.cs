using SharedKernel.Query.Specifications;
using System.Linq.Expressions;

namespace SharedKernel.Query.Builders
{
    public class EntityQueryBuilder<TEntity> where TEntity : class
    {
        internal readonly QueryState<TEntity> State;
        public EntityQueryBuilder(QueryState<TEntity> state)
        {
            State = state;
        }

        public EntityQueryBuilder<TEntity> Where(Expression<Func<TEntity, bool>> criteria)
        {
            State.Criteria = criteria;
            return this;
        }

        public EntityQueryBuilder<TEntity> With<TOptions>(TOptions options)
        {
            var builder = State.Resolver.GetCriteriaBuilder<TEntity, TOptions>();
            State.Criteria = builder?.Build(options) ?? (_ => false);
            return this;
        }

        public EntityOrderByBuilder<TEntity> OrderBy(Expression<Func<TEntity, object>> keySelector)
        {
            return new EntityOrderByBuilder<TEntity>(this, keySelector);
        }

        public ResultQueryBuilder<TEntity, TResult> As<TResult>()
        {
            State.ResultType = typeof(TResult);
            return new ResultQueryBuilder<TEntity, TResult>(State);
        }

        public ResultQueryBuilder<TEntity, TResult> As<TResult>(Expression<Func<TEntity, TResult>> selector)
        {
            State.ResultType = typeof(TResult);
            State.Selector = selector;
            return new ResultQueryBuilder<TEntity, TResult>(State);
        }


        public QuerySpecification<TEntity> Build()
        {
            return new QuerySpecification<TEntity>
            {
                Criteria = State.Criteria,
                OrderBy = BuildOrderBy()
            };
        }

        public QuerySpecification<TEntity, TResult> Build<TResult>()
        {
            var projectionBuilder = State.Resolver.GetProjectionBuilder<TEntity, TResult>();

            return new QuerySpecification<TEntity, TResult>
            {
                Criteria = State.Criteria,
                ResultCriteria = _ => true,
                Selector = projectionBuilder.Build()
            };
        }

        private Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? BuildOrderBy()
        {
            if (State.OrderKey is not Expression<Func<TEntity, object>> key)
                return null;

            return State.OrderDescending
                ? q => q.OrderByDescending(key)
                : q => q.OrderBy(key);
        }
    }
}
