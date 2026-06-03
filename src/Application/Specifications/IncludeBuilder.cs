using System.Linq.Expressions;

namespace Application.Specifications
{
    public class IncludeEntry<TEntity>
    {
        public LambdaExpression NavigationExpression { get; }

        public IncludeEntry<TEntity>? Next { get; private set; }

        public IncludeEntry(LambdaExpression navigationExpression)
        {
            NavigationExpression = navigationExpression;
        }

        internal void SetNext(IncludeEntry<TEntity> next)
        {
            Next = next;
        }
    }

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

    public class ReferenceThenIncludeBuilder<TEntity, TPrevious>
    {
        protected readonly IncludeEntry<TEntity> CurrentEntry;
        internal ReferenceThenIncludeBuilder(IncludeEntry<TEntity> currentEntry)
        {
            CurrentEntry = currentEntry;
        }

        public ReferenceThenIncludeBuilder<TEntity, TNext> ThenInclude<TNext>(Expression<Func<TPrevious, TNext>> expression)
        {
            var next = new IncludeEntry<TEntity>(expression);
            CurrentEntry.SetNext(next);
            return new ReferenceThenIncludeBuilder<TEntity, TNext>(next);
        }

        public CollectionThenIncludeBuilder<TEntity, TElement> ThenInclude<TElement>(Expression<Func<TPrevious, ICollection<TElement>>> expression)
        {
            var next = new IncludeEntry<TEntity>(expression);
            CurrentEntry.SetNext(next);
            return new CollectionThenIncludeBuilder<TEntity, TElement>(next);
        }

        public CollectionThenIncludeBuilder<TEntity, TElement> ThenInclude<TElement>(Expression<Func<TPrevious, IEnumerable<TElement>>> expression)
        {
            var next = new IncludeEntry<TEntity>(expression);
            CurrentEntry.SetNext(next);
            return new CollectionThenIncludeBuilder<TEntity, TElement>(next);
        }
    }

    public class CollectionThenIncludeBuilder<TEntity, TElement>
    {
        protected readonly IncludeEntry<TEntity> CurrentEntry;
        internal CollectionThenIncludeBuilder(IncludeEntry<TEntity> currentEntry)
        {
            CurrentEntry = currentEntry;
        }

        public ReferenceThenIncludeBuilder<TEntity, TNext>ThenInclude<TNext>(Expression<Func<TElement, TNext>> expression)
        {
            var next = new IncludeEntry<TEntity>(expression);
            CurrentEntry.SetNext(next);
            return new ReferenceThenIncludeBuilder<TEntity, TNext>(next);
        }

        public CollectionThenIncludeBuilder<TEntity, TNextElement> ThenInclude<TNextElement>(Expression<Func<TElement, IEnumerable<TNextElement>>> expression)
        {
            var next = new IncludeEntry<TEntity>(expression);
            CurrentEntry.SetNext(next);
            return new CollectionThenIncludeBuilder<TEntity, TNextElement>(next);
        }

        public CollectionThenIncludeBuilder<TEntity, TNextElement> ThenInclude<TNextElement>(Expression<Func<TElement, ICollection<TNextElement>>> expression)
        {
            var next = new IncludeEntry<TEntity>(expression);
            CurrentEntry.SetNext(next);
            return new CollectionThenIncludeBuilder<TEntity, TNextElement>(next);
        }
    }
}