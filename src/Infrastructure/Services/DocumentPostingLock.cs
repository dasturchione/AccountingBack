using Application.Abstractions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

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
}
