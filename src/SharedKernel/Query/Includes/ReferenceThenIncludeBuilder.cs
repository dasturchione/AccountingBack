using System.Linq.Expressions;

namespace SharedKernel.Query.Includes
{
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
}
