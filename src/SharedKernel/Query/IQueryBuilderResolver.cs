namespace SharedKernel.Query
{
    public interface IQueryBuilderResolver
    {
        ICriteriaBuilder<TEntity, TOptions>? GetCriteriaBuilder<TEntity, TOptions>();

        IProjectionBuilder<TEntity, TResult> GetProjectionBuilder<TEntity, TResult>();
    }
}
