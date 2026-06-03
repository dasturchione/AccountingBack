using Application.Specifications;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Common.Factory
{
    public class SpecificationFactory<TEntity> : ISpecificationFactory<TEntity> where TEntity : class
    {
        private readonly IServiceProvider _sp;
        public SpecificationFactory(IServiceProvider sp)
        {
            _sp = sp;
        }

        public QuerySpecification<TEntity> Build<TFilter>(TFilter filter)
        {
            var builder = _sp.GetRequiredService<IQuerySpecificationBuilder<TEntity, TFilter>>();
            return builder.Build(filter);
        }

        public QuerySpecification<TEntity, TResult> Build<TResult, TFilter>(TFilter filter)
        {
            var builder = _sp.GetRequiredService<IQuerySpecificationBuilder<TEntity, TResult, TFilter>>();
            return builder.Build(filter);
        }

        public PagedQuerySpecification<TEntity> BuildPaged<TFilter>(TFilter filter)
        {
            var builder = _sp.GetRequiredService<IPagedQuerySpecificationBuilder<TEntity, TFilter>>();
            return builder.Build(filter);
        }

        public PagedQuerySpecification<TEntity, TResult> BuildPaged<TResult, TFilter>(TFilter filter)
        {
            var builder = _sp.GetRequiredService<IPagedQuerySpecificationBuilder<TEntity, TResult, TFilter>>();
            return builder.Build(filter);
        }
    }
}
