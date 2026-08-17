using SharedKernel.Query.Specifications;
using System.Linq.Expressions;

namespace SharedKernel.Query.Builders
{
    public class ResultOrderByBuilder<TEntity, TResult> where TEntity : class
    {
        private readonly ResultQueryBuilder<TEntity, TResult> _parent;
        private readonly Expression<Func<TResult, object>> _keySelector;
        internal ResultOrderByBuilder(ResultQueryBuilder<TEntity, TResult> parent,
                                      Expression<Func<TResult, object>> keySelector)
        {
            _parent = parent;
            _keySelector = keySelector;
            _parent.OrderKey = keySelector;
        }

        public ResultQueryBuilder<TEntity, TResult> Desc()
        {
            _parent.OrderDescending = true;
            return _parent;
        }

        public QuerySpecification<TEntity, TResult> Build()
        {
            return _parent.Build();
        }

        public PagedQuerySpecification<TEntity, TResult> BuildPaged()
        {
            return _parent.BuildPaged();
        }

        public static implicit operator ResultQueryBuilder<TEntity, TResult>(
            ResultOrderByBuilder<TEntity, TResult> builder)
        {
            return builder._parent;
        }
    }
}
