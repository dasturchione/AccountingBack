using SharedKernel.Query.Specifications;
using System.Linq.Expressions;

namespace SharedKernel.Query.Builders
{
    public class EntityOrderByBuilder<TEntity> where TEntity : class
    {
        private readonly EntityQueryBuilder<TEntity> _parent;
        private readonly Expression<Func<TEntity, object>> _keySelector;
        internal EntityOrderByBuilder(EntityQueryBuilder<TEntity> parent,
                                      Expression<Func<TEntity, object>> keySelector)
        {
            _parent = parent;
            _keySelector = keySelector;
            _parent.State.OrderKey = keySelector;
        }

        public EntityQueryBuilder<TEntity> Desc()
        {
            _parent.State.OrderDescending = true;
            return _parent;
        }

        public QuerySpecification<TEntity> Build()
        {
            return _parent.Build();
        }

        public PagedQuerySpecification<TEntity> BuildPaged()
        {
            return _parent.BuildPaged();
        }

        public static implicit operator EntityQueryBuilder<TEntity>(EntityOrderByBuilder<TEntity> builder)
        {
            return builder._parent;
        }
    }
}
