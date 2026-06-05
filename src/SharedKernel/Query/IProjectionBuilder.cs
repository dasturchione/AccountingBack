using System.Linq.Expressions;

namespace SharedKernel.Query
{
    public interface IProjectionBuilder<TEntity, TResult>
    {
        Expression<Func<TEntity, TResult>> Build();
    }
}
