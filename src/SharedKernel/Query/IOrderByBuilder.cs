namespace SharedKernel.Query
{
    public interface IOrderByBuilder<TEntity, TResult>
    {
        Func<IQueryable<TResult>, IOrderedQueryable<TResult>> Build();
    }
}
