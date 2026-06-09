using SharedKernel.Filters;
using SharedKernel.Query;
using SharedKernel.Query.Builders;
using SharedKernel.Query.Specifications;
using System.Linq.Expressions;

namespace Infrastructure.Query
{
    public class QueryBuilder : IQueryBuilder
    {
        private readonly IQueryBuilderResolver _resolver;
        public QueryBuilder(IQueryBuilderResolver resolver)
        {
            _resolver = resolver;
        }

        public EntityQueryBuilder<TEntity> For<TEntity>() where TEntity : class =>
            new(new QueryState<TEntity> { Resolver = _resolver });

        public QuerySpecification<TEntity> Build<TEntity, TOptions>(TOptions options)
            where TEntity : class
        {
            var entityFilterBuilder = _resolver.GetCriteriaBuilder<TEntity, TOptions>();

            return new QuerySpecification<TEntity>
            {
                Criteria = SafeBuild(entityFilterBuilder, options)
            };
        }

        public QuerySpecification<TEntity, TResult> Build<TEntity, TResult, TOptions>(TOptions options)
            where TEntity : class
        {
            var entityFilterBuilder = _resolver.GetCriteriaBuilder<TEntity, TOptions>();
            var resultFilterBuilder = _resolver.GetCriteriaBuilder<TResult, TOptions>();
            var projectionBuilder = _resolver.GetProjectionBuilder<TEntity, TResult>();

            return new QuerySpecification<TEntity, TResult>
            {
                Selector = projectionBuilder.Build(),
                Criteria = SafeBuild(entityFilterBuilder, options),
                ResultCriteria = SafeBuild(resultFilterBuilder, options)
            };
        }

        public PagedQuerySpecification<TEntity> BuildPaged<TEntity, TOptions>(TOptions options)
            where TEntity : class
            where TOptions : IPaginationFilter
        {
            var entityFilterBuilder = _resolver.GetCriteriaBuilder<TEntity, TOptions>();
            var (take, skip) = CalculatePagination(options);

            return new PagedQuerySpecification<TEntity>
            {
                Criteria = SafeBuild(entityFilterBuilder, options),
                Take = take,
                Skip = skip
            };
        }

        public PagedQuerySpecification<TEntity, TResult> BuildPaged<TEntity, TResult, TOptions>(TOptions options)
                    where TEntity : class
                    where TOptions : IPaginationFilter
        {
            var entityFilterBuilder = _resolver.GetCriteriaBuilder<TEntity, TOptions>();
            var resultFilterBuilder = _resolver.GetCriteriaBuilder<TResult, TOptions>();
            var projectionBuilder = _resolver.GetProjectionBuilder<TEntity, TResult>();
            var (take, skip) = CalculatePagination(options);

            return new PagedQuerySpecification<TEntity, TResult>
            {
                Selector = projectionBuilder.Build(),
                Criteria = SafeBuild(entityFilterBuilder, options),
                ResultCriteria = SafeBuild(resultFilterBuilder, options),
                Take = take,
                Skip = skip
            };
        }

        private (int Take, int Skip) CalculatePagination(IPaginationFilter filter)
        {
            var defaultPageSize = 50;
            var take = filter.PageSize.GetValueOrDefault(defaultPageSize);
            var page = Math.Max(filter.Page, 1);
            return (take, (page - 1) * take);
        }

        private Expression<Func<T, bool>> SafeBuild<T, TFilter>(
            ICriteriaBuilder<T, TFilter>? builder, TFilter filter)
        {
            return builder?.Build(filter) ?? (_ => true);
        }
    }
}
