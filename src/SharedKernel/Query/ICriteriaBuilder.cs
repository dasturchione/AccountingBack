using System.Linq.Expressions;

namespace SharedKernel.Query
{
    public interface ICriteriaBuilder<TEntity, in TOptions>
    {
        Expression<Func<TEntity, bool>> Build(TOptions options);
    }
}
