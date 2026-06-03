using Application.Abstractions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction? _transaction;
        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public async Task BeginAsync(CancellationToken ct = default)
        {
            if (_transaction == null)
            {
                _transaction = await _context.Database.BeginTransactionAsync(ct);
            }
        }

        public async Task CommitAsync(CancellationToken ct = default)
        {
            try
            {
                if (_transaction != null)
                {
                    await _context.SaveChangesAsync(ct);
                    await _transaction.CommitAsync(ct);
                }
            }
            catch
            {
                await RollbackAsync(ct);
                throw;
            }
            finally
            {
                Dispose();
            }
        }

        public async Task RollbackAsync(CancellationToken ct = default)
        {
            if (_transaction is not null)
            {
                await _transaction.RollbackAsync(ct);
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        private void Dispose()
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }
}
