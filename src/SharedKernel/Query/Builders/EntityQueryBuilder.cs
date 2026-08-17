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

        public EntityQueryBuilder<TEntity> OrderBy(
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy)
        {
            State.OrderBy = orderBy;
            return this;
        }

        public EntityQueryBuilder<TEntity> IgnoreQueryFilters()
        {
            State.IgnoreQueryFilters = true;
            return this;
        }

        public EntityQueryBuilder<TEntity> AddIncludes(
            Action<SharedKernel.Query.Includes.IncludeBuilder<TEntity>> configure)
        {
            var includeBuilder = new SharedKernel.Query.Includes.IncludeBuilder<TEntity>();
            configure(includeBuilder);
            State.Includes.AddRange(includeBuilder.Entries);
            return this;
        }

        public EntityQueryBuilder<TEntity> Skip(int skip)
        {
            State.Skip = skip;
            return this;
        }

        public EntityQueryBuilder<TEntity> Take(int? take)
        {
            State.Take = take;
            return this;
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
            var specification = new QuerySpecification<TEntity>
            {
                Criteria = State.Criteria,
                OrderBy = BuildOrderBy(),
                IgnoreQueryFilters = State.IgnoreQueryFilters
            };

            AddIncludes(specification);
            return specification;
        }

        public QuerySpecification<TEntity, TResult> Build<TResult>()
        {
            var projectionBuilder = State.Resolver.GetProjectionBuilder<TEntity, TResult>();

            return new QuerySpecification<TEntity, TResult>
            {
                Criteria = State.Criteria,
                ResultCriteria = _ => true,
                Selector = projectionBuilder.Build(),
                IgnoreQueryFilters = State.IgnoreQueryFilters
            };
        }

        public PagedQuerySpecification<TEntity> BuildPaged()
        {
            var specification = new PagedQuerySpecification<TEntity>
            {
                Criteria = State.Criteria,
                OrderBy = BuildOrderBy(),
                IgnoreQueryFilters = State.IgnoreQueryFilters,
                Skip = State.Skip,
                Take = State.Take
            };

            AddIncludes(specification);
            return specification;
        }

        private Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? BuildOrderBy()
        {
            if (State.OrderBy is not null)
                return State.OrderBy;

            if (State.OrderKey is not Expression<Func<TEntity, object>> key)
                return null;

            return State.OrderDescending
                ? q => q.OrderByDescending(key)
                : q => q.OrderBy(key);
        }

        private void AddIncludes(QuerySpecification<TEntity> specification)
        {
            if (State.Includes.Count == 0)
                return;

            foreach (var include in State.Includes)
                specification.AddIncludeEntry(include);
        }
    }
}
