namespace Application.Abstractions.Integration.Edo;

/// <summary>
/// Checks local organization/provider configuration before an auth flow starts.
/// It never authenticates against a provider and never returns credential values.
/// </summary>
public interface IEdoAuthCredentialValidator
{
    Task ValidateAsync(EdoProviderCode providerCode, CancellationToken ct = default);
}
