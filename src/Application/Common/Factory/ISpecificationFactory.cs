using Application.Specifications;

namespace Application.Common.Factory
{
    public interface ISpecificationFactory<TEntity> where TEntity : class
    {
        QuerySpecification<TEntity> Build<TFilter>(TFilter filter);
        QuerySpecification<TEntity, TResult> Build<TResult, TFilter>(TFilter filter);

        PagedQuerySpecification<TEntity> BuildPaged<TFilter>(TFilter filter);
        PagedQuerySpecification<TEntity, TResult> BuildPaged<TResult, TFilter>(TFilter filter);
    }
}
