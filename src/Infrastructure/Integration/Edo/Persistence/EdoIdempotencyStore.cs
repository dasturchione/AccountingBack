using Application.Features.Integration.Edo;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SharedKernel.Exceptions;

namespace Integration.Edo.Persistence;

public sealed class EdoIdempotencyStore(AppDbContext context) : IEdoIdempotencyStore
{
    public Task<IdempotencyRecord?> GetAsync(
        int organizationId,
        string idempotencyKey,
        CancellationToken ct = default) =>
        context.IdempotencyRecords.SingleOrDefaultAsync(record =>
            record.OrganizationId == organizationId
            && record.IdempotencyKey == idempotencyKey, ct);

    public async Task AddAsync(IdempotencyRecord record, CancellationToken ct = default)
    {
        context.IdempotencyRecords.Add(record);
        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            context.ChangeTracker.Clear();
            throw new UniqueConstraintViolationException(
                "An EDO idempotency record with the same organization and key already exists.", ex);
        }
    }

    public Task UpdateAsync(IdempotencyRecord record, CancellationToken ct = default) =>
        context.SaveChangesAsync(ct);

    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is PostgresException postgresException
        && postgresException.SqlState == PostgresErrorCodes.UniqueViolation;
}
