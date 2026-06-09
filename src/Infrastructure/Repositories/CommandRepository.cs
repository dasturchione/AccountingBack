using Application.Abstractions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Exceptions;
using System.Linq.Expressions;

namespace Infrastructure.Repositories
{
    public class CommandRepository<TEntity> : ICommandRepository<TEntity> where TEntity : class
    {
        private readonly DbSet<TEntity> _dbSet;
        private readonly AppDbContext _context;

        public CommandRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<TEntity>();
        }

        public async Task CreateAsync(TEntity entity, CancellationToken ct = default)
        {
            try
            {
                await _dbSet.AddAsync(entity, ct);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new DbCommandException(ex);
            }
        }

        public async Task CreateAsync(IEnumerable<TEntity> entities, CancellationToken ct = default)
        {
            try
            {
                await _dbSet.AddRangeAsync(entities, ct);
                await _context.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                throw new DbCommandException(ex);
            }
        }

        public async Task DeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default)
        {
            try
            {
                await _dbSet.Where(predicate).ExecuteDeleteAsync(ct);
            }
            catch (Exception ex)
            {
                throw new DbCommandException(ex);
            }
        }

        public async Task DeleteAsync(TEntity entity, CancellationToken ct = default)
        {
            try
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                throw new DbCommandException(ex);
            }
        }

        public async Task DeleteAsync(IEnumerable<TEntity> entities, CancellationToken ct = default)
        {
            try
            {
                _dbSet.RemoveRange(entities);
                await _context.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                throw new DbCommandException(ex);
            }
        }

        public async Task ReloadAsync(TEntity entity, CancellationToken ct = default)
        {
            await _context.Entry(entity).ReloadAsync(ct);
        }

        public async Task UpdateAsync(TEntity entity, CancellationToken ct = default)
        {
            try
            {
                _dbSet.Update(entity);
                await _context.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                throw new DbCommandException(ex);
            }
        }

        public async Task UpdateAsync(IEnumerable<TEntity> entities, CancellationToken ct = default)
        {
            try
            {
                _dbSet.UpdateRange(entities);
                await _context.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                throw new DbCommandException(ex);
            }
        }
    }
}
