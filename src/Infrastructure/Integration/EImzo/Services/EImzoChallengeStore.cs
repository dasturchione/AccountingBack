using Application.Abstractions.Integration;
using Domain.Entities;
using Infrastructure.Persistence;
using Integration.Edocs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SharedKernel.Results;
using System.Security.Cryptography;
using System.Text;

namespace Integration.EImzo.Services;

/// <summary>
/// Database-backed, one-time challenge store shared by E-DOCS and the E-IMZO relay.
/// Only hashes and scope metadata are persisted; authId, signed bytes, tokens and keys never are.
/// Consumption uses a conditional ExecuteUpdate so two application instances cannot both consume
/// the same pending row.
/// </summary>
public sealed class EImzoChallengeStore(
    AppDbContext db,
    IOptions<EImzoSignatureRelayOptions> relayOptions,
    IOptions<EdocsOptions> edocsOptions,
    TimeProvider timeProvider) : IEdocsChallengeStore, IEImzoSignatureChallengeStore
{
    public TimeSpan ChallengeTtl =>
        TimeSpan.FromSeconds(Math.Clamp(
            Math.Min(relayOptions.Value.ChallengeTtlSeconds, edocsOptions.Value.ChallengeTtlSeconds),
            30,
            300));

    public async Task StoreAsync(EdocsChallengeRecord challenge, TimeSpan ttl, CancellationToken ct = default)
    {
        if (challenge is null
            || string.IsNullOrWhiteSpace(challenge.ChallengeId)
            || string.IsNullOrWhiteSpace(challenge.AuthIdHash))
            return;

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var expires = challenge.ExpiresAtUtc.UtcDateTime;
        if (expires <= now)
            return;

        var scope = new OrganizationScope(
            challenge.OrganizationId,
            Provider.EDocs,
            NormalizeTin(challenge.Tin),
            NormalizeSerial(challenge.SerialNumber));

        if (!IsValidScope(scope) || !IsHash(challenge.AuthIdHash))
            return;

        var boundedExpiry = expires < now.Add(ttl) ? expires : now.Add(ttl);

        db.EImzoChallenges.Add(new EImzoChallenge
        {
            ChallengeId = challenge.ChallengeId.Trim(),
            OrganizationId = scope.OrganizationId,
            Provider = scope.Provider,
            ExternalTin = scope.ExternalTin,
            EntityId = scope.EntityId!,
            AuthIdHash = challenge.AuthIdHash.Trim().ToLowerInvariant(),
            PayloadHash = null,
            SignMode = null,
            ExpiresAtUtc = boundedExpiry,
            ConsumedAtUtc = null,
            State = EImzoChallengeState.Pending,
            CreatedAtUtc = now
        });

        await db.SaveChangesAsync(ct);
    }

    public async Task StoreAsync(EImzoSignatureChallenge challenge, CancellationToken ct = default)
    {
        if (challenge is null
            || string.IsNullOrWhiteSpace(challenge.ChallengeId)
            || string.IsNullOrWhiteSpace(challenge.PayloadHash))
            return;

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var expires = challenge.ExpiresAtUtc.UtcDateTime;
        if (expires <= now)
            return;

        var serial = NormalizeSerial(challenge.SerialNumber);
        var scope = ProviderScopeCanonicalizer.Canonicalize(challenge.Scope);
        var entityId = string.IsNullOrWhiteSpace(scope.EntityId) ? serial : scope.EntityId;
        if (serial is null || !string.Equals(entityId, serial, StringComparison.Ordinal))
            return;
        scope = scope with { EntityId = entityId };

        if (!IsValidScope(scope) || !IsHash(challenge.PayloadHash))
            return;

        var boundedExpiry = expires < now.Add(ChallengeTtl) ? expires : now.Add(ChallengeTtl);

        db.EImzoChallenges.Add(new EImzoChallenge
        {
            ChallengeId = challenge.ChallengeId.Trim(),
            OrganizationId = scope.OrganizationId,
            Provider = scope.Provider,
            ExternalTin = NormalizeTin(scope.ExternalTin),
            EntityId = entityId!,
            AuthIdHash = null,
            PayloadHash = challenge.PayloadHash.Trim(),
            SignMode = challenge.Mode.ToString(),
            ExpiresAtUtc = boundedExpiry,
            ConsumedAtUtc = null,
            State = EImzoChallengeState.Pending,
            CreatedAtUtc = now
        });

        await db.SaveChangesAsync(ct);
    }

    public Task<EdocsChallengeRecord?> ConsumeAsync(
        string challengeId,
        OrganizationScope scope,
        string serialNumber,
        CancellationToken ct = default) =>
        ConsumeEdocsCoreAsync(challengeId, scope, serialNumber, ct);

    public async Task<EImzoSignatureChallenge?> ConsumeSignatureAsync(
        string challengeId,
        OrganizationScope scope,
        string serialNumber,
        CancellationToken ct = default)
    {
        var row = await ConsumeRowAsync(challengeId, scope, serialNumber, ct);
        if (row is null || row.PayloadHash is null || row.SignMode is null)
            return null;

        if (!Enum.TryParse<Pkcs7SignMode>(row.SignMode, out var mode))
            return null;

        return new EImzoSignatureChallenge(
            row.ChallengeId,
            new OrganizationScope(row.OrganizationId, row.Provider, row.ExternalTin, row.EntityId),
            row.EntityId,
            row.PayloadHash,
            mode,
            new DateTimeOffset(row.ExpiresAtUtc, TimeSpan.Zero));
    }

    private async Task<EdocsChallengeRecord?> ConsumeEdocsCoreAsync(
        string challengeId,
        OrganizationScope scope,
        string serialNumber,
        CancellationToken ct)
    {
        var row = await ConsumeRowAsync(challengeId, scope, serialNumber, ct);
        return row?.AuthIdHash is null
            ? null
            : new EdocsChallengeRecord(
                row.ChallengeId,
                row.OrganizationId,
                row.ExternalTin,
                row.EntityId,
                row.AuthIdHash,
                new DateTimeOffset(row.ExpiresAtUtc, TimeSpan.Zero));
    }

    private async Task<EImzoChallenge?> ConsumeRowAsync(
        string challengeId,
        OrganizationScope scope,
        string serialNumber,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(challengeId)
            || string.IsNullOrWhiteSpace(serialNumber)
            || scope is null
            || !IsValidScope(scope))
            return null;

        var canonicalScope = ProviderScopeCanonicalizer.Canonicalize(scope);
        var canonicalSerial = NormalizeSerial(serialNumber);
        if (canonicalSerial is null)
            return null;

        var entityId = string.IsNullOrWhiteSpace(canonicalScope.EntityId)
            ? canonicalSerial
            : canonicalScope.EntityId;
        if (string.IsNullOrWhiteSpace(entityId)
            || !string.Equals(entityId, canonicalSerial, StringComparison.Ordinal))
            return null;

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var normalizedChallengeId = challengeId.Trim();
        var normalizedTin = NormalizeTin(canonicalScope.ExternalTin);

        await db.EImzoChallenges
            .Where(x =>
                x.ChallengeId == normalizedChallengeId
                && x.OrganizationId == canonicalScope.OrganizationId
                && x.Provider == canonicalScope.Provider
                && x.ExternalTin == normalizedTin
                && x.EntityId == entityId
                && x.State == EImzoChallengeState.Pending
                && x.ConsumedAtUtc == null
                && x.ExpiresAtUtc <= now)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.State, EImzoChallengeState.Expired), ct);

        var candidate = await db.EImzoChallenges
            .AsNoTracking()
            .SingleOrDefaultAsync(x =>
                x.ChallengeId == normalizedChallengeId
                && x.OrganizationId == canonicalScope.OrganizationId
                && x.Provider == canonicalScope.Provider
                && x.ExternalTin == normalizedTin
                && x.EntityId == entityId
                && x.State == EImzoChallengeState.Pending
                && x.ConsumedAtUtc == null
                && x.ExpiresAtUtc > now, ct);

        if (candidate is null)
            return null;

        var updated = await db.EImzoChallenges
            .Where(x =>
                x.Id == candidate.Id
                && x.ChallengeId == normalizedChallengeId
                && x.OrganizationId == canonicalScope.OrganizationId
                && x.Provider == canonicalScope.Provider
                && x.ExternalTin == normalizedTin
                && x.EntityId == entityId
                && x.State == EImzoChallengeState.Pending
                && x.ConsumedAtUtc == null
                && x.ExpiresAtUtc > now)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.State, EImzoChallengeState.Consumed)
                .SetProperty(x => x.ConsumedAtUtc, now), ct);

        return updated == 1 ? candidate : null;
    }

    private static bool IsValidScope(OrganizationScope scope) =>
        scope.OrganizationId > 0
        && Enum.IsDefined(scope.Provider)
        && IsDigitsOnly(scope.ExternalTin)
        && !string.IsNullOrWhiteSpace(scope.EntityId);

    private static string NormalizeTin(string? value) =>
        value is null ? string.Empty : new(value.Where(char.IsDigit).ToArray());

    private static string? NormalizeSerial(string? value) =>
        ProviderScopeCanonicalizer.NormalizeEdocsEntityId(value);

    private static bool IsHash(string value)
    {
        if (value.Length == 64 && value.All(Uri.IsHexDigit))
            return true;

        Span<byte> decoded = stackalloc byte[32];
        return Convert.TryFromBase64String(value, decoded, out var written) && written == 32;
    }

    private static bool IsDigitsOnly(string value) =>
        value.Length > 0 && value.All(char.IsAsciiDigit);
}
