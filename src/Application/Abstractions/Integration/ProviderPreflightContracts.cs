using Domain.Entities;
using SharedKernel.Results;

namespace Application.Abstractions.Integration;

public enum ProviderContractState
{
    Verified,
    PartiallyVerified,
    Blocked
}

/// <summary>Safe local-only provider readiness result. It deliberately contains no TIN or secret material.</summary>
public sealed record ProviderPreflightResult(
    string Provider,
    bool Ready,
    bool CredentialPresent,
    bool SessionPresent,
    DateTime? SessionExpiresAtUtc,
    bool NeedsReauthentication,
    string ContractState,
    string? SafeErrorCode);

public interface IProviderPreflightService
{
    Task<Result<ProviderPreflightResult>> GetAsync(Provider provider, CancellationToken ct = default);
}
