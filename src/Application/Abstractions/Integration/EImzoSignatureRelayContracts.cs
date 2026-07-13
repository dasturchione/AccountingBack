using Domain.Entities;
using SharedKernel.Results;

namespace Application.Abstractions.Integration;

/// <summary>
/// Safety limits for the shared E-IMZO signature relay. These are foundation defaults only — no
/// certificate, key path, password or provider endpoint is configured here.
/// </summary>
public sealed class EImzoSignatureRelayOptions
{
    /// <summary>Maximum accepted PKCS#7 size in bytes. Oversized submissions are rejected fail-closed.</summary>
    public int MaxPkcs7Bytes { get; set; } = 256 * 1024;

    /// <summary>Requested challenge lifetime; clamped to [30, 300] seconds by the store.</summary>
    public int ChallengeTtlSeconds { get; set; } = 120;
}

/// <summary>
/// Request for a canonical payload. <paramref name="ChallengeContent"/> is the provider-specific
/// value the holder will sign (e.g. an E-DOCS authId); it is opaque to the relay and never a secret.
/// </summary>
public sealed record EImzoCanonicalPayloadRequest(
    OrganizationScope Scope,
    string SerialNumber,
    string ChallengeContent);

/// <summary>
/// Canonical bytes to be signed, produced only by a provider policy. <see cref="Content"/> lives in
/// request memory; only <see cref="PayloadHash"/> (a SHA-256 digest) and <see cref="Mode"/> are ever
/// persisted in a challenge.
/// </summary>
public sealed record EImzoCanonicalPayload(
    byte[] Content,
    string PayloadHash,
    Pkcs7SignMode Mode);

/// <summary>
/// Safe, persistable challenge metadata. Contains no signature bytes, no token and no key material —
/// only the payload hash, serial, scope, provider-decided mode and expiry.
/// </summary>
public sealed record EImzoSignatureChallenge(
    string ChallengeId,
    OrganizationScope Scope,
    string SerialNumber,
    string PayloadHash,
    Pkcs7SignMode Mode,
    DateTimeOffset ExpiresAtUtc);

/// <summary>
/// A submitted signature. <see cref="Pkcs7"/> is the raw signature and must stay in request memory:
/// the relay only measures its length and never copies it to a store, cache, log or ProblemDetails.
/// </summary>
public sealed record EImzoSignatureSubmission(
    string ChallengeId,
    OrganizationScope Scope,
    string SerialNumber,
    string PayloadHash,
    ReadOnlyMemory<byte> Pkcs7);

/// <summary>Result of accepting a signature — safe metadata only (byte length, never the bytes).</summary>
public sealed record AcceptedSignature(
    string ChallengeId,
    int OrganizationId,
    Provider Provider,
    string SerialNumber,
    string PayloadHash,
    Pkcs7SignMode Mode,
    int SignatureByteLength,
    DateTimeOffset AcceptedAtUtc);

/// <summary>
/// Provider-specific canonicalization policy. The relay owns no canonical format itself; a provider
/// whose canonical payload/mode is not yet known returns <c>UnsupportedContract</c>.
/// </summary>
public interface IEImzoProviderSignaturePolicy
{
    Provider Provider { get; }
    Result<EImzoCanonicalPayload> CreateCanonicalPayload(EImzoCanonicalPayloadRequest request);
}

/// <summary>
/// One-time, TTL-bound store for challenge metadata. <see cref="TakeAsync"/> consumes the challenge so
/// a replayed challenge id resolves to <c>null</c>. Implementations persist only safe metadata.
/// </summary>
public interface IEImzoSignatureChallengeStore
{
    TimeSpan ChallengeTtl { get; }
    Task StoreAsync(EImzoSignatureChallenge challenge, CancellationToken ct = default);
    Task<EImzoSignatureChallenge?> ConsumeSignatureAsync(
        string challengeId,
        OrganizationScope scope,
        string serialNumber,
        CancellationToken ct = default);
}

/// <summary>
/// Shared E-IMZO signature relay used by provider integrations. It canonicalizes only through provider
/// policy, issues one-time TTL challenges, and accepts a submitted signature strictly bound to its
/// challenge (scope, serial, payload hash, mode, expiry) — all fail-closed. It performs no network I/O
/// and is deliberately independent of the server-side <see cref="IEImzoSigner"/>.
/// </summary>
public interface IEImzoSignatureRelay
{
    Result<EImzoCanonicalPayload> CreateCanonicalPayload(EImzoCanonicalPayloadRequest request);

    Task<Result<EImzoSignatureChallenge>> IssueChallengeAsync(
        OrganizationScope scope,
        string serialNumber,
        EImzoCanonicalPayload payload,
        CancellationToken ct = default);

    Result ValidateChallengeBinding(EImzoSignatureSubmission submission, EImzoSignatureChallenge issued);

    Task<Result<AcceptedSignature>> AcceptSignatureAsync(
        EImzoSignatureSubmission submission,
        CancellationToken ct = default);
}
