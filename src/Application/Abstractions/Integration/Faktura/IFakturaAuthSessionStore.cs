using Application.Abstractions.Integration.Edo;

namespace Application.Abstractions.Integration.Faktura;

public interface IFakturaAuthSessionStore
{
    Task SaveAsync(
        FakturaAuthSession session,
        CancellationToken ct = default);

    Task<FakturaAuthSession?> GetAsync(
        FakturaAuthSessionScope scope,
        CancellationToken ct = default);

    Task RemoveAsync(
        FakturaAuthSessionScope scope,
        CancellationToken ct = default);
}

public sealed record FakturaAuthSessionScope(
    int UserId,
    int OrganizationId,
    EdoProviderCode ProviderCode);

public sealed record FakturaAuthCookie(
    string Name,
    string Value,
    DateTimeOffset? ExpiresAt);

public sealed record FakturaAuthSession(
    FakturaAuthSessionScope Scope,
    IReadOnlyCollection<FakturaAuthCookie> Cookies,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt);
