using SharedKernel.Filters;
using SharedKernel.Query.Builders;
using SharedKernel.Query.Specifications;

namespace SharedKernel.Query
{
    public interface IQueryBuilder
    {
        EntityQueryBuilder<TEntity> For<TEntity>() where TEntity : class;

        QuerySpecification<TEntity> Build<TEntity, TOptions>(TOptions options) where TEntity : class;

        QuerySpecification<TEntity, TResult> Build<TEntity, TResult, TOptions>(TOptions options) where TEntity : class;

        PagedQuerySpecification<TEntity> BuildPaged<TEntity, TOptions>(TOptions options)
            where TEntity : class
            where TOptions : IPaginationFilter;

        PagedQuerySpecification<TEntity, TResult> BuildPaged<TEntity, TResult, TOptions>(TOptions options)
            where TEntity : class
            where TOptions : IPaginationFilter;
    }
}
