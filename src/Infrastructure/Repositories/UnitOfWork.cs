using Application.Abstractions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction? _transaction;
        private int _depth;
        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public async Task BeginAsync(CancellationToken ct = default)
        {
            if (_transaction == null)
            {
                _transaction = await _context.Database.BeginTransactionAsync(ct);
                _depth = 1;
                return;
            }

            _depth++;
        }

        public Task SaveChangesAsync(CancellationToken ct = default)
        {
            return _context.SaveChangesAsync(ct);
        }

        public async Task CommitAsync(CancellationToken ct = default)
        {
            var shouldDispose = false;

            try
            {
                if (_transaction == null)
                    return;

                if (_depth > 1)
                {
                    _depth--;
                    return;
                }

                if (_context.ChangeTracker.HasChanges())
                    await _context.SaveChangesAsync(ct);

                await _transaction.CommitAsync(ct);
                shouldDispose = true;
            }
            catch
            {
                await RollbackAsync(ct);
                throw;
            }
            finally
            {
                if (shouldDispose)
                    await DisposeAsync();
            }
        }

        public async Task RollbackAsync(CancellationToken ct = default)
        {
            if (_transaction is not null)
            {
                await _transaction.RollbackAsync(ct);
                await _transaction.DisposeAsync();
                _transaction = null;
                _depth = 0;
            }
        }

        private async Task DisposeAsync()
        {
            if (_transaction is null)
                return;

            await _transaction.DisposeAsync();
            _transaction = null;
            _depth = 0;
        }
    }
}
