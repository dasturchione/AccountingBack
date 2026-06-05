using SharedKernel.Filters;
using SharedKernel.Query.Specifications;

namespace SharedKernel.Query
{
    public interface IQueryBuilder<TEntity> where TEntity : class
    {
        QuerySpecification<TEntity> Build<TOptions>(TOptions options);

        QuerySpecification<TEntity, TResult> Build<TResult, TOptions>(TOptions options);

        PagedQuerySpecification<TEntity> BuildPaged<TOptions>(TOptions options) where TOptions : IPaginationFilter;

        PagedQuerySpecification<TEntity, TResult> BuildPaged<TResult, TOptions>(TOptions options) where TOptions : IPaginationFilter;
    }
}
