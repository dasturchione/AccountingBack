using Microsoft.EntityFrameworkCore;

namespace Application.Common.Extensions;

public static class QueryableExtensions
{
    public static Task<List<T>> ToListAsyncSafe<T>(this IQueryable<T> source, CancellationToken cancellationToken = default)
    {
        return source is IAsyncEnumerable<T>
            ? source.ToListAsync(cancellationToken)
            : Task.FromResult(source.ToList());
    }
}
