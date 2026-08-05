using Integration.Didox.Configs;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Integration.Didox.Http;

/// <summary>
/// Didox user-key tokenlarini organization bo'yicha ajratib, DataProtection bilan
/// shifrlangan persistent faylda saqlaydi. Token plaintext sifatida diskka yozilmaydi.
/// </summary>
public sealed class DidoxTokenCache
{
    private const string ProtectorPurpose = "accounting-back/didox/user-key/v1";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IDataProtector _protector;
    private readonly DidoxTokenStorageOptions _options;
    private readonly string _rootPath;
    private readonly object _gate = new();

    public DidoxTokenCache(
        IDataProtectionProvider dataProtectionProvider,
        IOptions<DidoxTokenStorageOptions> options,
        IHostEnvironment hostEnvironment)
    {
        _protector = dataProtectionProvider.CreateProtector(ProtectorPurpose);
        _options = options.Value;
        _rootPath = ResolveRootPath(_options.RootPath, hostEnvironment.ContentRootPath);
        Directory.CreateDirectory(_rootPath);
    }

    public bool TryGet(int organizationId, out string token)
    {
        token = string.Empty;
        if (organizationId <= 0)
            return false;

        PersistedToken? persisted;
        try
        {
            persisted = Read(organizationId);
        }
        catch (Exception ex) when (ex is CryptographicException
                                   or JsonException
                                   or IOException
                                   or UnauthorizedAccessException
                                   or InvalidOperationException)
        {
            return false;
        }

        if (persisted is null
            || persisted.OrganizationId != organizationId
            || string.IsNullOrWhiteSpace(persisted.Token)
            || persisted.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            if (persisted is not null && persisted.ExpiresAt <= DateTimeOffset.UtcNow)
                TryDelete(GetPath(organizationId));

            return false;
        }

        token = persisted.Token;
        return true;
    }

    public void Set(int organizationId, string token, TimeSpan validFor)
    {
        if (organizationId <= 0)
            throw new ArgumentOutOfRangeException(nameof(organizationId));
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("A non-empty Didox token is required.", nameof(token));
        if (validFor <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(validFor));

        var persisted = new PersistedToken
        {
            OrganizationId = organizationId,
            Token = token,
            ExpiresAt = DateTimeOffset.UtcNow.Add(validFor)
        };

        var protectedBytes = _protector.Protect(JsonSerializer.SerializeToUtf8Bytes(persisted, JsonOptions));
        var path = GetPath(organizationId);
        var temporaryPath = path + "." + Convert.ToHexString(RandomNumberGenerator.GetBytes(8)) + ".tmp";

        lock (_gate)
        {
            try
            {
                File.WriteAllBytes(temporaryPath, protectedBytes);
                File.Move(temporaryPath, path, overwrite: true);
            }
            finally
            {
                TryDelete(temporaryPath);
            }
        }
    }

    public void Clear(int organizationId)
    {
        if (organizationId > 0)
            TryDelete(GetPath(organizationId));
    }

    public void Clear()
    {
        foreach (var path in Directory.EnumerateFiles(_rootPath, "*.bin"))
            TryDelete(path);
    }

    private PersistedToken? Read(int organizationId)
    {
        var path = GetPath(organizationId);
        if (!File.Exists(path))
            return null;

        var protectedBytes = File.ReadAllBytes(path);
        if (protectedBytes.Length > _options.MaxTokenFileBytes)
            throw new InvalidOperationException("Didox token storage entry exceeds the configured size limit.");

        var json = _protector.Unprotect(protectedBytes);
        return JsonSerializer.Deserialize<PersistedToken>(json, JsonOptions);
    }

    private string GetPath(int organizationId)
    {
        var scopeHash = Convert.ToHexString(SHA256.HashData(
            Encoding.UTF8.GetBytes($"DIDOX:{organizationId}")));
        return Path.Combine(_rootPath, scopeHash + ".bin");
    }

    private static string ResolveRootPath(string configuredPath, string contentRootPath) =>
        Path.IsPathRooted(configuredPath)
            ? Path.GetFullPath(configuredPath)
            : Path.GetFullPath(Path.Combine(contentRootPath, configuredPath));

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private sealed class PersistedToken
    {
        public int OrganizationId { get; init; }
        public string Token { get; init; } = string.Empty;
        public DateTimeOffset ExpiresAt { get; init; }
    }
}
