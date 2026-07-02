using Application.Abstractions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
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
                await _context.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (TryTranslateUniqueConstraint(ex, out var translated))
            {
                throw translated;
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
            catch (DbUpdateException ex) when (TryTranslateUniqueConstraint(ex, out var translated))
            {
                throw translated;
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
            catch (DbUpdateConcurrencyException ex)
            {
                throw new OptimisticConcurrencyException("The record was modified by another transaction. Please reload and try again.", ex);
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
            catch (DbUpdateConcurrencyException ex)
            {
                throw new OptimisticConcurrencyException("One or more records were modified by another transaction. Please reload and try again.", ex);
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
            catch (DbUpdateConcurrencyException ex)
            {
                throw new OptimisticConcurrencyException("The record was modified by another transaction. Please reload and try again.", ex);
            }
            catch (DbUpdateException ex) when (TryTranslateUniqueConstraint(ex, out var translated))
            {
                throw translated;
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
            catch (DbUpdateConcurrencyException ex)
            {
                throw new OptimisticConcurrencyException("One or more records were modified by another transaction. Please reload and try again.", ex);
            }
            catch (DbUpdateException ex) when (TryTranslateUniqueConstraint(ex, out var translated))
            {
                throw translated;
            }
            catch (Exception ex)
            {
                throw new DbCommandException(ex);
            }
        }

        private static bool TryTranslateUniqueConstraint(DbUpdateException ex, out Exception translated)
        {
            translated = null!;

            if (ex.InnerException is not PostgresException postgresException || postgresException.SqlState != PostgresErrorCodes.UniqueViolation)
                return false;

            translated = postgresException.ConstraintName switch
            {
                "ux_inv_inventory_count_doc_active_warehouse" =>
                    new UniqueConstraintViolationException("Another active inventory count already exists for this warehouse.", ex),
                "ux_inv_product_table_marking_number_active" =>
                    new UniqueConstraintViolationException("Active marking number must be unique.", ex),
                "ux_inv_product_table_serial_number_active" =>
                    new UniqueConstraintViolationException("Active serial number must be unique.", ex),
                _ => new UniqueConstraintViolationException(postgresException.Detail ?? postgresException.MessageText, ex)
            };

            return true;
        }
    }
}
