using System.Linq.Expressions;

namespace Application.Abstractions
{
    public interface IProjectionMap<TEntity, TResult>
    {
        Expression<Func<TEntity, TResult>> Build();
    }

    public interface IProjectionMap<TEntity, TResult, in TParam>
    {
        Expression<Func<TEntity, TResult>> Build(TParam param);
    }
}
