using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Query;

namespace Infrastructure.Query
{
    public class QueryBuilderResolver : IQueryBuilderResolver
    {
        private readonly IServiceProvider _sp;
        public QueryBuilderResolver(IServiceProvider sp) => _sp = sp;

        public ICriteriaBuilder<TEntity, TOptions>? GetCriteriaBuilder<TEntity, TOptions>() =>
            _sp.GetService<ICriteriaBuilder<TEntity, TOptions>>();

        public IOrderByBuilder<TEntity, TResult>? GetOrderByBuilder<TEntity, TResult>() =>
            _sp.GetService<IOrderByBuilder<TEntity, TResult>>();

        public IProjectionBuilder<TEntity, TResult> GetProjectionBuilder<TEntity, TResult>() =>
            _sp.GetRequiredService<IProjectionBuilder<TEntity, TResult>>();
    }
}
