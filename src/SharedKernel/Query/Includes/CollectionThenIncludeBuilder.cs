using System.Linq.Expressions;

namespace SharedKernel.Query.Includes
{
    public class CollectionThenIncludeBuilder<TEntity, TElement>
    {
        protected readonly IncludeEntry<TEntity> CurrentEntry;
        internal CollectionThenIncludeBuilder(IncludeEntry<TEntity> currentEntry)
        {
            CurrentEntry = currentEntry;
        }

        public ReferenceThenIncludeBuilder<TEntity, TNext> ThenInclude<TNext>(Expression<Func<TElement, TNext>> expression)
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
