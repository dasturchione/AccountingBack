using System.Linq.Expressions;

namespace SharedKernel.Query.Includes
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
}
