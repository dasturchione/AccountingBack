namespace Application.Abstractions;

/// <summary>
/// Stages entity changes in the current DbContext. This repository never saves;
/// the caller owns the transaction and calls <see cref="IUnitOfWork.SaveChangesAsync"/>.
/// </summary>
public interface ITrackingRepository<TEntity> where TEntity : class
{
    Task AddAsync(TEntity entity, CancellationToken ct = default);
    Task UpdateAsync(TEntity entity, CancellationToken ct = default);
}
