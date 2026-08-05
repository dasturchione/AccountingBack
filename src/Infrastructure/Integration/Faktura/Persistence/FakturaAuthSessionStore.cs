using Application.Abstractions.Authentication;
using Application.Abstractions.Integration.Edo;
using Application.Abstractions.Integration.Faktura;
using Integration.Faktura.Configs;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Integration.Faktura.Persistence;

public sealed class FakturaAuthSessionStore : IFakturaAuthSessionStore
{
    private const string ProtectorPurpose = "accounting-back/faktura/auth-session/v1";
    private const string AccountCookieName = ".AspNet.AccountFakturaApp";
    private const string FormsCookieName = ".ASPXAUTH";

    private static readonly HashSet<string> SupportedCookieNames =
    [
        AccountCookieName,
        FormsCookieName
    ];

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IUserContext _userContext;
    private readonly IDataProtector _protector;
    private readonly TimeProvider _timeProvider;
    private readonly FakturaAuthSessionStorageOptions _options;
    private readonly string _rootPath;
    private readonly SemaphoreSlim _writeLock = new(1, 1);

    public FakturaAuthSessionStore(
        IUserContext userContext,
        IDataProtectionProvider dataProtectionProvider,
        IOptions<FakturaAuthSessionStorageOptions> options,
        IHostEnvironment hostEnvironment,
        TimeProvider timeProvider)
    {
        _userContext = userContext;
        _protector = dataProtectionProvider.CreateProtector(ProtectorPurpose);
        _options = options.Value;
        _timeProvider = timeProvider;
        _rootPath = ResolveRootPath(_options.RootPath, hostEnvironment.ContentRootPath);
        Directory.CreateDirectory(_rootPath);
    }

    public async Task SaveAsync(
        FakturaAuthSession session,
        CancellationToken ct = default)
    {
        EnsureScope(session.Scope);
        EnsureFakturaProvider(session.Scope.ProviderCode);

        var now = _timeProvider.GetUtcNow();
        var sessionExpiry = Min(
            session.ExpiresAt,
            now.AddMinutes(_options.SessionLifetimeMinutes));

        var cookies = session.Cookies
            .Where(cookie => SupportedCookieNames.Contains(cookie.Name))
            .Where(cookie => !string.IsNullOrWhiteSpace(cookie.Value))
            .Where(cookie => cookie.ExpiresAt is null || cookie.ExpiresAt > now)
            .Select(cookie => new PersistedCookie
            {
                Name = cookie.Name,
                Value = cookie.Value,
                ExpiresAt = cookie.ExpiresAt
            })
            .ToArray();

        if (sessionExpiry <= now || !HasCompleteCookieSet(cookies.Select(cookie => cookie.Name)))
            throw new InvalidOperationException("Faktura authentication session is expired or has no valid cookies.");

        var persisted = new PersistedSession
        {
            UserId = session.Scope.UserId,
            OrganizationId = session.Scope.OrganizationId,
            ProviderCode = session.Scope.ProviderCode.ToString(),
            CreatedAt = session.CreatedAt,
            ExpiresAt = sessionExpiry,
            Cookies = cookies
        };

        var protectedBytes = _protector.Protect(JsonSerializer.SerializeToUtf8Bytes(persisted, JsonOptions));
        var path = GetPath(session.Scope);
        var temporaryPath = path + "." + Convert.ToHexString(RandomNumberGenerator.GetBytes(8)) + ".tmp";

        await _writeLock.WaitAsync(ct);
        try
        {
            await File.WriteAllBytesAsync(temporaryPath, protectedBytes, ct);
            File.Move(temporaryPath, path, overwrite: true);
        }
        finally
        {
            TryDelete(temporaryPath);
            _writeLock.Release();
        }
    }

    public async Task<FakturaAuthSession?> GetAsync(
        FakturaAuthSessionScope scope,
        CancellationToken ct = default)
    {
        EnsureScope(scope);
        EnsureFakturaProvider(scope.ProviderCode);

        var path = GetPath(scope);
        if (!File.Exists(path))
            return null;

        var protectedBytes = await ReadBoundedAsync(path, ct);
        PersistedSession persisted;
        try
        {
            var json = _protector.Unprotect(protectedBytes);
            persisted = JsonSerializer.Deserialize<PersistedSession>(json, JsonOptions)
                ?? throw new InvalidOperationException();
        }
        catch (Exception ex) when (ex is CryptographicException or JsonException or InvalidOperationException)
        {
            throw new InvalidOperationException("Faktura authentication session could not be read.");
        }

        if (!MatchesScope(persisted, scope))
            return null;

        var now = _timeProvider.GetUtcNow();
        if (persisted.ExpiresAt <= now)
            return null;

        var cookies = (persisted.Cookies ?? [])
            .Where(cookie => SupportedCookieNames.Contains(cookie.Name))
            .Where(cookie => !string.IsNullOrWhiteSpace(cookie.Value))
            .Where(cookie => cookie.ExpiresAt is null || cookie.ExpiresAt > now)
            .Select(cookie => new FakturaAuthCookie(cookie.Name, cookie.Value, cookie.ExpiresAt))
            .ToArray();

        if (!HasCompleteCookieSet(cookies.Select(cookie => cookie.Name)))
            return null;

        return new FakturaAuthSession(
            scope,
            cookies,
            persisted.CreatedAt,
            persisted.ExpiresAt);
    }

    public Task RemoveAsync(
        FakturaAuthSessionScope scope,
        CancellationToken ct = default)
    {
        EnsureScope(scope);
        EnsureFakturaProvider(scope.ProviderCode);
        ct.ThrowIfCancellationRequested();
        TryDelete(GetPath(scope));
        return Task.CompletedTask;
    }

    private async Task<byte[]> ReadBoundedAsync(string path, CancellationToken ct)
    {
        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            options: FileOptions.Asynchronous | FileOptions.SequentialScan);

        using var memory = new MemoryStream();
        var buffer = new byte[4096];
        while (true)
        {
            var bytesRead = await stream.ReadAsync(buffer, ct);
            if (bytesRead == 0)
                break;

            if (memory.Length + bytesRead > _options.MaxSessionFileBytes)
                throw new InvalidOperationException("Faktura authentication session exceeds the configured size limit.");

            memory.Write(buffer, 0, bytesRead);
        }

        return memory.ToArray();
    }

    private void EnsureScope(FakturaAuthSessionScope scope)
    {
        if (scope.UserId <= 0 || scope.OrganizationId <= 0)
            throw new InvalidOperationException("Faktura authentication session scope is invalid.");

        if (_userContext.Id != scope.UserId
            || _userContext.OrganizationId != scope.OrganizationId)
        {
            throw new InvalidOperationException("Faktura authentication session is outside the current user or organization scope.");
        }
    }

    private static void EnsureFakturaProvider(EdoProviderCode providerCode)
    {
        if (providerCode != EdoProviderCode.FAKTURA)
            throw new InvalidOperationException("The Faktura authentication session store accepts only the Faktura provider.");
    }

    private string GetPath(FakturaAuthSessionScope scope)
    {
        var scopeKey = $"{scope.UserId}:{scope.OrganizationId}:{scope.ProviderCode}";
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(scopeKey)));
        return Path.Combine(_rootPath, hash + ".bin");
    }

    private static bool MatchesScope(
        PersistedSession persisted,
        FakturaAuthSessionScope scope) =>
        persisted.UserId == scope.UserId
        && persisted.OrganizationId == scope.OrganizationId
        && string.Equals(
            persisted.ProviderCode,
            scope.ProviderCode.ToString(),
            StringComparison.Ordinal);

    private static bool HasCompleteCookieSet(IEnumerable<string> cookieNames) =>
        SupportedCookieNames.All(name => cookieNames.Contains(name, StringComparer.Ordinal));

    private static string ResolveRootPath(string configuredPath, string contentRootPath) =>
        Path.IsPathRooted(configuredPath)
            ? Path.GetFullPath(configuredPath)
            : Path.GetFullPath(Path.Combine(contentRootPath, configuredPath));

    private static DateTimeOffset Min(DateTimeOffset left, DateTimeOffset right) =>
        left <= right ? left : right;

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch (IOException)
        {
            // Expired session cleanup is best-effort; a later read remains fail-closed.
        }
        catch (UnauthorizedAccessException)
        {
            // Storage permission errors must not expose session contents.
        }
    }

    private sealed class PersistedSession
    {
        public int UserId { get; init; }
        public int OrganizationId { get; init; }
        public string ProviderCode { get; init; } = string.Empty;
        public DateTimeOffset CreatedAt { get; init; }
        public DateTimeOffset ExpiresAt { get; init; }
        public PersistedCookie[]? Cookies { get; init; } = [];
    }

    private sealed class PersistedCookie
    {
        public string Name { get; init; } = string.Empty;
        public string Value { get; init; } = string.Empty;
        public DateTimeOffset? ExpiresAt { get; init; }
    }
}
