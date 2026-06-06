using System.Linq.Expressions;

namespace Application.Abstractions;

public interface ICommandRepository<TEntity> where TEntity : class
{
    Task CreateAsync(TEntity entity, CancellationToken ct = default);
    Task CreateAsync(IEnumerable<TEntity> entities, CancellationToken ct = default);
    Task UpdateAsync(TEntity entity, CancellationToken ct = default);
    Task UpdateAsync(IEnumerable<TEntity> entities, CancellationToken ct = default);
    Task DeleteAsync(TEntity entity, CancellationToken ct = default);
    Task DeleteAsync(IEnumerable<TEntity> entities, CancellationToken ct = default);
    Task DeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);
    Task ReloadAsync(TEntity entity, CancellationToken ct = default);
}
