using Application.Abstractions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace Infrastructure.Services;

public class DocumentPostingLock : IDocumentPostingLock
{
    private readonly AppDbContext _context;

    public DocumentPostingLock(AppDbContext context)
    {
        _context = context;
    }

    public Task AcquireAsync(short documentTypeId, long documentId, CancellationToken ct = default)
    {
        var lockKey = unchecked(((long)documentTypeId << 48) ^ documentId);
        return _context.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock({lockKey})", ct);
    }

    public async Task<bool> TryAcquireAsync(short documentTypeId, long documentId, CancellationToken ct = default)
    {
        var lockKey = unchecked(((long)documentTypeId << 48) ^ documentId);
        var connection = _context.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;

        if (shouldClose)
            await connection.OpenAsync(ct);

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT pg_try_advisory_xact_lock(@lockKey)";

            var parameter = command.CreateParameter();
            parameter.ParameterName = "@lockKey";
            parameter.Value = lockKey;
            command.Parameters.Add(parameter);

            var currentTransaction = _context.Database.CurrentTransaction?.GetDbTransaction();
            if (currentTransaction is not null)
                command.Transaction = currentTransaction;

            var result = await command.ExecuteScalarAsync(ct);
            return result is bool acquired && acquired;
        }
        finally
        {
            if (shouldClose)
                await connection.CloseAsync();
        }
    }
}
