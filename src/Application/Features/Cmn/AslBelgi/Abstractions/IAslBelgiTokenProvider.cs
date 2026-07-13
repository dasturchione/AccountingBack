using SharedKernel.Results;

namespace Application.Features.Cmn.AslBelgi.Abstractions;

/// <summary>
/// Supplies a currently-valid Asl Belgisi access token.
/// In business mode it returns the configured long-lived apiKey; in technical mode it returns a
/// cached access token, transparently refreshing (or re-authenticating) when it expires.
/// Callers no longer read the token from the request header.
/// </summary>
public interface IAslBelgiTokenProvider
{
    Task<Result<string>> GetAccessTokenAsync(CancellationToken ct = default);

    Task<Result<string>> RefreshAfterUnauthorizedAsync(string failedAccessToken, CancellationToken ct = default);
}
