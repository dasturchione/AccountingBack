namespace Application.Specifications
{
    public interface IQuerySpecificationBuilder<TEntity, TFilter> where TEntity : class
    {
        QuerySpecification<TEntity> Build(TFilter filter);
    }

    public interface IQuerySpecificationBuilder<TEntity, TResult, TFilter> where TEntity : class
    {
        QuerySpecification<TEntity, TResult> Build(TFilter filter);
    }
}
