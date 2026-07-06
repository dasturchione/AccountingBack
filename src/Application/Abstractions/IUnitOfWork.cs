namespace Application.Abstractions;

public interface IUnitOfWork
{
    Task BeginAsync(CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default) => Task.CompletedTask;
    Task CommitAsync(CancellationToken ct = default);
    Task RollbackAsync(CancellationToken ct = default);
}
