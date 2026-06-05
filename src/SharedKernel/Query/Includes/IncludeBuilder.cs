using System.Linq.Expressions;

namespace SharedKernel.Query.Includes
{
    public class IncludeBuilder<TEntity> where TEntity : class
    {
        private readonly List<IncludeEntry<TEntity>> _entries = [];
        internal IReadOnlyList<IncludeEntry<TEntity>> Entries => _entries;

        public ReferenceThenIncludeBuilder<TEntity, TProperty> Include<TProperty>(Expression<Func<TEntity, TProperty>> expression)
        {
            var entry = new IncludeEntry<TEntity>(expression);
            _entries.Add(entry);
            return new ReferenceThenIncludeBuilder<TEntity, TProperty>(entry);
        }

        public CollectionThenIncludeBuilder<TEntity, TElement> Include<TElement>(Expression<Func<TEntity, ICollection<TElement>>> expression)
        {
            var entry = new IncludeEntry<TEntity>(expression);
            _entries.Add(entry);
            return new CollectionThenIncludeBuilder<TEntity, TElement>(entry);
        }

        public CollectionThenIncludeBuilder<TEntity, TElement> Include<TElement>(Expression<Func<TEntity, IEnumerable<TElement>>> expression)
        {
            var entry = new IncludeEntry<TEntity>(expression);
            _entries.Add(entry);
            return new CollectionThenIncludeBuilder<TEntity, TElement>(entry);
        }
    }
}
