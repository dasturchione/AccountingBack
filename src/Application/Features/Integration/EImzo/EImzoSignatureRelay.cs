using System.Security.Cryptography;
using System.Text;
using Application.Abstractions.Integration;
using Domain.Entities;
using SharedKernel.Results;

namespace Application.Features.Integration.EImzo;

/// <summary>
/// Shared E-IMZO signature relay. Canonicalizes only through a registered provider policy, issues
/// one-time TTL challenges (metadata only), and accepts a raw PKCS#7 that is strictly bound to its
/// challenge. The raw signature is never persisted or logged — only its length is reported. The relay
/// deliberately takes no dependency on the server-side <see cref="IEImzoSigner"/> and performs no I/O
/// beyond the challenge store.
/// </summary>
public sealed class EImzoSignatureRelay(
    IEnumerable<IEImzoProviderSignaturePolicy> policies,
    IEImzoSignatureChallengeStore challengeStore,
    TimeProvider timeProvider,
    EImzoSignatureRelayOptions options) : IEImzoSignatureRelay
{
    private readonly Dictionary<Provider, IEImzoProviderSignaturePolicy> _policies =
        policies.GroupBy(p => p.Provider).ToDictionary(g => g.Key, g => g.Last());

    public Result<EImzoCanonicalPayload> CreateCanonicalPayload(EImzoCanonicalPayloadRequest request)
    {
        if (request is null || ValidateScope(request.Scope) is { IsSuccess: false } invalid)
            return Result.Failure<EImzoCanonicalPayload>(EImzoSignatureRelayErrors.ScopeInvalid);

        if (string.IsNullOrWhiteSpace(request.SerialNumber))
            return Result.Failure<EImzoCanonicalPayload>(EImzoSignatureRelayErrors.SerialRequired);

        // The relay owns no canonical format: an unknown provider contract fails closed.
        if (!_policies.TryGetValue(request.Scope.Provider, out var policy))
            return Result.Failure<EImzoCanonicalPayload>(EImzoSignatureRelayErrors.UnsupportedContract);

        return policy.CreateCanonicalPayload(request);
    }

    public async Task<Result<EImzoSignatureChallenge>> IssueChallengeAsync(
        OrganizationScope scope,
        string serialNumber,
        EImzoCanonicalPayload payload,
        CancellationToken ct = default)
    {
        if (ValidateScope(scope) is { IsSuccess: false } invalid)
            return Result.Failure<EImzoSignatureChallenge>(invalid.Error);

        if (string.IsNullOrWhiteSpace(serialNumber))
            return Result.Failure<EImzoSignatureChallenge>(EImzoSignatureRelayErrors.SerialRequired);

        if (payload is null || string.IsNullOrWhiteSpace(payload.PayloadHash))
            return Result.Failure<EImzoSignatureChallenge>(EImzoSignatureRelayErrors.ChallengeContentRequired);

        var expiresAt = timeProvider.GetUtcNow().Add(challengeStore.ChallengeTtl);
        var challenge = new EImzoSignatureChallenge(
            CreateChallengeId(),
            scope,
            serialNumber.Trim(),
            payload.PayloadHash,
            payload.Mode,
            expiresAt);

        await challengeStore.StoreAsync(challenge, ct);
        return Result.Success(challenge);
    }

    public Result ValidateChallengeBinding(EImzoSignatureSubmission submission, EImzoSignatureChallenge issued)
    {
        if (submission is null || issued is null)
            return Result.Failure(EImzoSignatureRelayErrors.ChallengeInvalid);

        if (!Equals(submission.ChallengeId, issued.ChallengeId))
            return Result.Failure(EImzoSignatureRelayErrors.ChallengeMismatch);

        if (!ScopeEquals(submission.Scope, issued.Scope, submission.SerialNumber))
            return Result.Failure(EImzoSignatureRelayErrors.OrganizationMismatch);

        if (!FixedTimeEquals(submission.SerialNumber, issued.SerialNumber))
            return Result.Failure(EImzoSignatureRelayErrors.SerialMismatch);

        if (!FixedTimeEquals(submission.PayloadHash, issued.PayloadHash))
            return Result.Failure(EImzoSignatureRelayErrors.PayloadMismatch);

        if (issued.ExpiresAtUtc <= timeProvider.GetUtcNow())
            return Result.Failure(EImzoSignatureRelayErrors.ChallengeExpired);

        return Result.Success();
    }

    public async Task<Result<AcceptedSignature>> AcceptSignatureAsync(
        EImzoSignatureSubmission submission,
        CancellationToken ct = default)
    {
        if (submission is null || ValidateScope(submission.Scope) is { IsSuccess: false })
            return Result.Failure<AcceptedSignature>(EImzoSignatureRelayErrors.ScopeInvalid);

        if (string.IsNullOrWhiteSpace(submission.ChallengeId)
            || string.IsNullOrWhiteSpace(submission.SerialNumber)
            || string.IsNullOrWhiteSpace(submission.PayloadHash))
            return Result.Failure<AcceptedSignature>(EImzoSignatureRelayErrors.ChallengeInvalid);

        // Size is checked before anything else so an oversized body is never processed further.
        if (submission.Pkcs7.Length == 0)
            return Result.Failure<AcceptedSignature>(EImzoSignatureRelayErrors.SignatureMissing);

        if (submission.Pkcs7.Length > Math.Max(1, options.MaxPkcs7Bytes))
            return Result.Failure<AcceptedSignature>(EImzoSignatureRelayErrors.SignatureTooLarge);

        // One-time consume: a replayed challenge id resolves to null here and fails closed.
        var issued = await challengeStore.ConsumeSignatureAsync(
            submission.ChallengeId,
            submission.Scope,
            submission.SerialNumber,
            ct);
        if (issued is null)
            return Result.Failure<AcceptedSignature>(EImzoSignatureRelayErrors.ChallengeInvalid);

        if (ValidateChallengeBinding(submission, issued) is { IsSuccess: false } invalidBinding)
            return Result.Failure<AcceptedSignature>(invalidBinding.Error);

        // Only safe metadata leaves this method; the raw PKCS#7 stays in the caller's request memory.
        return Result.Success(new AcceptedSignature(
            issued.ChallengeId,
            issued.Scope.OrganizationId,
            issued.Scope.Provider,
            issued.SerialNumber,
            issued.PayloadHash,
            issued.Mode,
            submission.Pkcs7.Length,
            timeProvider.GetUtcNow()));
    }

    private static Result ValidateScope(OrganizationScope scope)
    {
        if (scope is null || scope.OrganizationId <= 0)
            return Result.Failure(EImzoSignatureRelayErrors.ScopeInvalid);

        if (!Enum.IsDefined(scope.Provider))
            return Result.Failure(EImzoSignatureRelayErrors.ScopeInvalid);

        if (!IsDigitsOnly(scope.ExternalTin))
            return Result.Failure(EImzoSignatureRelayErrors.ScopeInvalid);

        if (scope.EntityId is not null && string.IsNullOrWhiteSpace(scope.EntityId))
            return Result.Failure(EImzoSignatureRelayErrors.ScopeInvalid);

        return Result.Success();
    }

    private static bool ScopeEquals(OrganizationScope left, OrganizationScope right, string serialNumber) =>
        left.OrganizationId == right.OrganizationId
        && left.Provider == right.Provider
        && FixedTimeEquals(left.ExternalTin, right.ExternalTin)
        && string.Equals(
            left.EntityId ?? serialNumber.Trim(),
            right.EntityId ?? serialNumber.Trim(),
            StringComparison.Ordinal);

    private static bool FixedTimeEquals(string? left, string? right) =>
        CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes((left ?? string.Empty).Trim()),
            Encoding.UTF8.GetBytes((right ?? string.Empty).Trim()));

    private static bool IsDigitsOnly(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        foreach (var ch in value)
        {
            if (!char.IsAsciiDigit(ch))
                return false;
        }

        return true;
    }

    private static string CreateChallengeId() =>
        Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
}
