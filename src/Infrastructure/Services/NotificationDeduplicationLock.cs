using System.Security.Cryptography;
using System.Text;
using Application.Abstractions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public sealed class NotificationDeduplicationLock : INotificationDeduplicationLock
{
    private readonly AppDbContext _context;

    public NotificationDeduplicationLock(AppDbContext context)
    {
        _context = context;
    }

    public Task AcquireAsync(string key, CancellationToken ct = default)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        var lockKey = BitConverter.ToInt64(hash, 0);
        return _context.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock({lockKey})", ct);
    }
}
