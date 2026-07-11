namespace Application.Abstractions.Integration;

/// <summary>
/// Result of a Didox company-token exchange.
/// The E-IMZO signature is produced on the frontend; the backend only relays it to Didox.
/// </summary>
public sealed class DidoxTokenResultDto
{
    public bool IsSuccessful { get; init; }
    public string? Token { get; init; }
    public string? Message { get; init; }
}

/// <summary>
/// Exchanges an E-IMZO signature (or password) for a Didox company token (user-key).
/// The signature is created by the client via E-IMZO — this client never signs anything.
/// </summary>
public interface IDidoxAuthClient
{
    Task<DidoxTokenResultDto> GetTokenBySignatureAsync(string taxId, string signature, string? locale, CancellationToken ct = default);

    Task<DidoxTokenResultDto> GetTokenByPasswordAsync(string taxId, string password, string? locale, CancellationToken ct = default);
}

/// <summary>
/// Didox document lifecycle operations beyond the generic submit/status/cancel — currently signing.
/// The signature is produced by the frontend E-IMZO flow; the backend only relays it with the
/// standard 2-header (user-key + Partner-Authorization) auth.
/// </summary>
public interface IDidoxDocumentClient
{
    Task<TaxProviderOperationResultDto> SignAsync(string documentId, string signature, string? companyToken, CancellationToken ct = default);
}
