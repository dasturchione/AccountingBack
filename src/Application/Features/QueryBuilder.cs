using Application.Specifications;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Filters;
using System.Linq.Expressions;

/// <summary>
/// будущее улучшение: добавить кэширование построенных спецификаций, чтобы избежать повторного построения для одинаковых фильтров. Это может значительно повысить производительность при частом использовании одних и тех же фильтров. Кэш можно реализовать с помощью ConcurrentDictionary, где ключом будет хэш фильтра, а значением - построенная спецификация.
/// </summary>
namespace Application.Features
{
    /// <summary>
    /// Интерфейс для построения спецификаций запросов на основе входных фильтров. Позволяет создавать как простые, так и проекционные и пагинированные спецификации. Использует внедрение зависимостей для получения необходимых билдера фильтров и проекций. Это обеспечивает гибкость и расширяемость при добавлении новых типов фильтров и проекций в будущем.
    /// </summary>
    public interface IQueryBuilder<TEntity> where TEntity : class
    {
        QuerySpecification<TEntity> Build<TFilter>(TFilter filter);

        QuerySpecification<TEntity, TResult> Build<TResult, TFilter>(TFilter filter);

        PagedQuerySpecification<TEntity> BuildPaged<TFilter>(TFilter filter) where TFilter : IPaginationFilter;

        PagedQuerySpecification<TEntity, TResult> BuildPaged<TResult, TFilter>(TFilter filter) where TFilter : IPaginationFilter;
    }

    public class QueryBuilder<TEntity> : IQueryBuilder<TEntity> where TEntity : class
    {
        private readonly IServiceProvider _sp;
        public QueryBuilder(IServiceProvider sp)
        {
            _sp = sp;
        }

        public QuerySpecification<TEntity> Build<TFilter>(TFilter filter)
        {
            var entityFilterBuilder = _sp.GetService<IFilterBuilder<TEntity, TFilter>>();

            return new QuerySpecification<TEntity>
            {
                Criteria = SafeBuild(entityFilterBuilder, filter)
            };
        }

        public QuerySpecification<TEntity, TResult> Build<TResult, TFilter>(TFilter filter)
        {
            var entityFilterBuilder = _sp.GetService<IFilterBuilder<TEntity, TFilter>>();
            var resultFilterBuilder = _sp.GetService<IFilterBuilder<TResult, TFilter>>();
            var projectionBuilder = _sp.GetRequiredService<IProjectionBuilder<TEntity, TResult>>();

            return new QuerySpecification<TEntity, TResult>
            {
                Selector = projectionBuilder.Build(),
                Criteria = SafeBuild(entityFilterBuilder, filter),
                ResultCriteria = SafeBuild(resultFilterBuilder, filter)
            };
        }

        public PagedQuerySpecification<TEntity> BuildPaged<TFilter>(TFilter filter) where TFilter : IPaginationFilter
        {
            var entityFilterBuilder = _sp.GetService<IFilterBuilder<TEntity, TFilter>>();
            var (take, skip) = CalculatePagination(filter);

            return new PagedQuerySpecification<TEntity>
            {
                Criteria = SafeBuild(entityFilterBuilder, filter),
                Take = take,
                Skip = skip
            };
        }

        public PagedQuerySpecification<TEntity, TResult> BuildPaged<TResult, TFilter>(TFilter filter) where TFilter : IPaginationFilter
        {
            var entityFilterBuilder = _sp.GetService<IFilterBuilder<TEntity, TFilter>>();
            var resultFilterBuilder = _sp.GetService<IFilterBuilder<TResult, TFilter>>();
            var projectionBuilder = _sp.GetRequiredService<IProjectionBuilder<TEntity, TResult>>();

            var (take, skip) = CalculatePagination(filter);

            return new PagedQuerySpecification<TEntity, TResult>
            {
                Selector = projectionBuilder.Build(),
                Criteria = SafeBuild(entityFilterBuilder, filter),
                ResultCriteria = SafeBuild(resultFilterBuilder, filter),
                Take = take,
                Skip = skip
            };
        }

        private static (int Take, int Skip) CalculatePagination(IPaginationFilter filter)
        {
            var take = filter.PageSize.GetValueOrDefault(50);
            var page = Math.Max(filter.Page, 1);

            return (take, (page - 1) * take);
        }

        private static Expression<Func<T, bool>> SafeBuild<T, TFilter>(IFilterBuilder<T, TFilter>? builder, TFilter filter)
        {
            return builder?.Build(filter) ?? (_ => true);
        }
    }

    public interface IFilterBuilder<TEntity, in TFilter>
    {
        Expression<Func<TEntity, bool>> Build(TFilter filter);
    }

    public interface IProjectionBuilder<TEntity, TResult>
    {
        Expression<Func<TEntity, TResult>> Build();
    }
}
