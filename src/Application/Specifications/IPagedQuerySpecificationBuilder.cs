namespace Application.Specifications
{
    public interface IPagedQuerySpecificationBuilder<TEntity, TFilter> where TEntity : class
    {
        PagedQuerySpecification<TEntity> Build(TFilter filter);
    }

    public interface IPagedQuerySpecificationBuilder<TEntity, TResult, TFilter> where TEntity : class
    {
        PagedQuerySpecification<TEntity, TResult> Build(TFilter filter);
    }
}
