using System.Security.Cryptography;
using System.Text;

namespace Application.Abstractions.Integration.Edo;

public static class EdoProviderProductIdentity
{
    private static readonly IReadOnlySet<string> ExplicitItemTypeCatalogCodes =
        new HashSet<string>(StringComparer.Ordinal)
        {
            "10306002005000000",
            "10304008002000000",
            "10704008002000000"
        };

    public static bool RequiresExplicitMapping(string? catalogCode) =>
        Normalize(catalogCode) is { } normalized
        && ExplicitItemTypeCatalogCodes.Contains(normalized);

    public static string? Create(
        string? providerCode,
        string? catalogCode,
        string? packageCode,
        string? providerProductName,
        bool? isService)
    {
        var provider = Normalize(providerCode);
        var catalog = Normalize(catalogCode);
        var name = Normalize(providerProductName);
        if (provider is null || catalog is null || name is null || !isService.HasValue)
            return null;

        var nameHash = Hash(name);
        return Hash(string.Join('\u001f',
            provider,
            catalog,
            Normalize(packageCode) ?? string.Empty,
            nameHash,
            isService.Value ? "SERVICE" : "GOODS"));
    }

    public static string HashName(string providerProductName) =>
        Hash(Normalize(providerProductName)
            ?? throw new ArgumentException("Provider product name is required.", nameof(providerProductName)));

    public static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string Hash(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
}
