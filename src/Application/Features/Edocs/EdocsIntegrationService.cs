using Application.Abstractions;
using Application.Abstractions.Integration;
using Domain.Entities;
using SharedKernel.Results;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Application.Features.Edocs;

/// <summary>
/// E-DOCS vertical slice wired into the shared credential/session foundation. After a client-side
/// E-IMZO login the certificate <em>identity</em> (serial metadata only — never the private key, PFX
/// or E-IMZO password) is ensured as a <see cref="ProviderCredential"/> of kind
/// <see cref="CredentialKind.EImzoCertificate"/>, and the session token is envelope-encrypted through
/// <see cref="ISecretProtector"/> and stored in <see cref="IProviderSessionStore"/> bound to that
/// credential's id/version. The plaintext token stays in request memory and never reaches a response,
/// DB, cache, log or error. The existing challenge store and authId canonical payload are untouched.
/// </summary>
public sealed class EdocsIntegrationService(
    IOrganizationScopeResolver scopeResolver,
    ISecretProtector secretProtector,
    IProviderCredentialStore credentialStore,
    IProviderSessionStore sessionStore,
    IEdocsClient edocsClient,
    IEdocsChallengeStore challengeStore,
    IUnitOfWork unitOfWork,
    EdocsSessionOptions sessionOptions) : IEdocsIntegrationService
{
    private const string EImzoCertificateKind = "edocs-eimzo-cert";

    public async Task<Result<EdocsChallengeResponse>> CreateChallengeAsync(EdocsChallengeRequest request, CancellationToken ct = default)
    {
        var serialNumber = ProviderScopeCanonicalizer.NormalizeEdocsEntityId(request.SerialNumber);
        if (serialNumber is null)
            return Result.Failure<EdocsChallengeResponse>(Error.Problem("Edocs.SerialNumberRequired", "E-IMZO serial number is required."));

        var scope = await scopeResolver.ResolveAsync(Provider.EDocs, ct: ct);
        if (!scope.IsSuccess)
            return Result.Failure<EdocsChallengeResponse>(scope.Error);

        var authId = await edocsClient.GetAuthIdAsync(serialNumber, ct);
        if (!authId.IsSuccess)
            return Result.Failure<EdocsChallengeResponse>(authId.Error);

        var expiresAt = DateTimeOffset.UtcNow.Add(challengeStore.ChallengeTtl);
        var challenge = new EdocsChallengeRecord(
            CreateChallengeId(),
            scope.Value.OrganizationId,
            scope.Value.ExternalTin,
            serialNumber,
            HashValue(authId.Value),
            expiresAt);

        await challengeStore.StoreAsync(challenge, expiresAt - DateTimeOffset.UtcNow, ct);
        return Result.Success(new EdocsChallengeResponse
        {
            ChallengeId = challenge.ChallengeId,
            AuthId = authId.Value,
            ExpiresAtUtc = challenge.ExpiresAtUtc
        });
    }

    public async Task<Result<EdocsLoginResponse>> LoginAsync(EdocsLoginRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.ChallengeId) || string.IsNullOrWhiteSpace(request.SerialNumber) || string.IsNullOrWhiteSpace(request.Pkcs7))
            return Result.Failure<EdocsLoginResponse>(Error.Problem("Edocs.LoginPayloadInvalid", "Challenge id, serial number and PKCS#7 are required."));

        var serialNumber = ProviderScopeCanonicalizer.NormalizeEdocsEntityId(request.SerialNumber);
        if (serialNumber is null)
            return Result.Failure<EdocsLoginResponse>(Error.Problem("Edocs.LoginPayloadInvalid", "Challenge id, serial number and PKCS#7 are required."));

        var scope = await scopeResolver.ResolveAsync(Provider.EDocs, ct: ct);
        if (!scope.IsSuccess)
            return Result.Failure<EdocsLoginResponse>(scope.Error);

        var challenge = await challengeStore.ConsumeAsync(
            request.ChallengeId,
            scope.Value with { EntityId = serialNumber },
            serialNumber,
            ct);
        if (challenge is null)
            return Result.Failure<EdocsLoginResponse>(Error.Conflict("Edocs.ChallengeInvalid", "The E-DOCS challenge is expired, already used or does not match this organization."));

        // The raw PKCS#7 is passed straight through and never persisted or logged. Deliberately no
        // retry: a repeated login could create another external session/token.
        var login = await edocsClient.LoginAsync(challenge.SerialNumber, request.Pkcs7, ct);
        if (!login.IsSuccess)
            return Result.Failure<EdocsLoginResponse>(login.Error);

        if (login.Value.EntityTins.Count > 0 && !login.Value.EntityTins.Any(x => SameTin(x, scope.Value.ExternalTin)))
            return Result.Failure<EdocsLoginResponse>(Error.Forbidden("Edocs.EntityTinMismatch", "The E-DOCS login entity does not match the current organization."));

        var profile = await edocsClient.GetProfileAsync(login.Value.Token, ct);
        if (!profile.IsSuccess)
            return Result.Failure<EdocsLoginResponse>(profile.Error);

        if (!SameTin(profile.Value.Tin, scope.Value.ExternalTin))
            return Result.Failure<EdocsLoginResponse>(Error.Forbidden("Edocs.ProfileTinMismatch", "The E-DOCS profile does not match the current organization."));

        // Credential and session use the same canonical certificate-serial scope.
        var sessionScope = scope.Value with { EntityId = challenge.SerialNumber };
        var credential = await EnsureCertificateCredentialAsync(sessionScope, challenge.SerialNumber, ct);
        if (!credential.IsSuccess)
            return Result.Failure<EdocsLoginResponse>(credential.Error);

        // Envelope-encrypt the session token bound to the trusted E-DOCS scope; persist only the
        // opaque reference plus a non-reversible fingerprint. The plaintext never leaves memory.
        var tokenReference = secretProtector.Protect(sessionScope, login.Value.Token);
        if (!tokenReference.IsSuccess)
            return Result.Failure<EdocsLoginResponse>(tokenReference.Error);

        var material = new ProviderSessionMaterial(
            credential.Value.CredentialId,
            tokenReference.Value,
            RefreshTokenReference: null,
            TokenFingerprint: Fingerprint(login.Value.Token),
            // Enforced locally from Edocs:TokenTtlMinutes; an expired session fails closed as
            // "reconnect required" without ever calling the provider. No refresh flow exists.
            AccessExpiresAtUtc: DateTime.UtcNow.Add(sessionOptions.AccessTokenTtl),
            RefreshExpiresAtUtc: null,
            Credential: credential.Value.Entity);

        var session = await sessionStore.SetAsync(sessionScope, material, credential.Value.Version, ct);
        if (!session.IsSuccess)
            return Result.Failure<EdocsLoginResponse>(session.Error);

        try
        {
            // Path 2: credential and session are staged above and committed together exactly once.
            await unitOfWork.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Failure<EdocsLoginResponse>(LoginConcurrencyConflict);
        }
        catch (DbUpdateException)
        {
            // Includes the unique-scope race between parallel credential/session ensures. Do not
            // expose provider/database details to the caller.
            return Result.Failure<EdocsLoginResponse>(LoginConcurrencyConflict);
        }
        return Result.Success(new EdocsLoginResponse { Connected = true });
    }

    public async Task<Result<EdocsProfileResponse>> GetProfileAsync(CancellationToken ct = default)
    {
        var scope = await ResolveEdocsSessionScopeAsync(ct);
        if (!scope.IsSuccess)
            return Result.Failure<EdocsProfileResponse>(scope.Error);

        var session = await LoadUsableSessionAsync(scope.Value, ct);
        if (!session.IsSuccess)
            return Result.Failure<EdocsProfileResponse>(session.Error);

        using var secret = session.Value.Secret;
        var token = Encoding.UTF8.GetString(secret.Value);
        var profile = await edocsClient.GetProfileAsync(token, ct);
        if (!profile.IsSuccess)
        {
            await RevokeOnUnauthorizedAsync(profile.Error, scope.Value, token, ct);
            return Result.Failure<EdocsProfileResponse>(profile.Error);
        }

        if (!SameTin(profile.Value.Tin, scope.Value.ExternalTin))
        {
            await sessionStore.RemoveIfMatchesAsync(scope.Value, Fingerprint(token), ct);
            await unitOfWork.SaveChangesAsync(ct);
            return Result.Failure<EdocsProfileResponse>(Error.Forbidden("Edocs.ProfileTinMismatch", "The E-DOCS profile does not match the current organization."));
        }

        return Result.Success(new EdocsProfileResponse { Tin = scope.Value.ExternalTin, Name = profile.Value.Name });
    }

    public async Task<Result<EdocsDocumentListResponse>> GetDocumentsAsync(EdocsDocumentListQuery query, CancellationToken ct = default)
    {
        if (query.Page < 1 || query.Limit is < 1 or > 100)
            return Result.Failure<EdocsDocumentListResponse>(Error.Problem("Edocs.DocumentPagingInvalid", "Page must be positive and limit must be between 1 and 100."));

        // Only page/limit are contract-safe today. The remaining query fields have no verified
        // provider contract, so they are rejected instead of being passed through. The error is
        // deliberately value-free.
        if (!string.IsNullOrWhiteSpace(query.Sort)
            || query.Order is not null
            || !string.IsNullOrWhiteSpace(query.Filter)
            || !string.IsNullOrWhiteSpace(query.Fields)
            || !string.IsNullOrWhiteSpace(query.Io)
            || !string.IsNullOrWhiteSpace(query.Status)
            || !string.IsNullOrWhiteSpace(query.Type))
            return Result.Failure<EdocsDocumentListResponse>(Error.Problem(
                "Edocs.DocumentFilterUnverified",
                "Only page and limit are supported until the E-DOCS document filter contract is verified."));

        var scope = await ResolveEdocsSessionScopeAsync(ct);
        if (!scope.IsSuccess)
            return Result.Failure<EdocsDocumentListResponse>(scope.Error);

        var session = await LoadUsableSessionAsync(scope.Value, ct);
        if (!session.IsSuccess)
            return Result.Failure<EdocsDocumentListResponse>(session.Error);

        using var secret = session.Value.Secret;
        var token = Encoding.UTF8.GetString(secret.Value);
        var documents = await edocsClient.GetDocumentsAsync(token, query, ct);
        if (!documents.IsSuccess)
        {
            await RevokeOnUnauthorizedAsync(documents.Error, scope.Value, token, ct);
            return Result.Failure<EdocsDocumentListResponse>(documents.Error);
        }

        return Result.Success(new EdocsDocumentListResponse
        {
            Total = documents.Value.Total,
            Items = documents.Value.Items.Select(x => new EdocsDocumentListItem
            {
                Id = x.Id,
                Number = x.Number,
                Type = x.Type,
                Status = x.Status,
                Date = x.Date
            }).ToList()
        });
    }

    // Ensures a single active E-IMZO certificate-identity credential for the serial-scoped credential.
    // The stored reference is a protected certificate-identity marker, never key material.
    private async Task<Result<(long CredentialId, int Version, ProviderCredential Entity)>> EnsureCertificateCredentialAsync(
        OrganizationScope sessionScope,
        string serialNumber,
        CancellationToken ct)
    {
        var canonicalSerial = ProviderScopeCanonicalizer.NormalizeEdocsEntityId(serialNumber);
        if (canonicalSerial is null)
            return Result.Failure<(long, int, ProviderCredential)>(Error.Business(
                "Edocs.CertificateSerialInvalid", "The certificate serial is invalid."));

        var credentialScope = sessionScope with { EntityId = canonicalSerial };

        var existing = await credentialStore.GetAsync(credentialScope, CredentialKind.EImzoCertificate, ct);
        if (!existing.IsSuccess)
            return Result.Failure<(long, int, ProviderCredential)>(existing.Error);

        if (existing.Value is not null)
            return Result.Success((existing.Value.Id, existing.Value.KeyVersion, existing.Value));

        var identityReference = secretProtector.Protect(credentialScope, $"{EImzoCertificateKind}:{canonicalSerial}");
        if (!identityReference.IsSuccess)
            return Result.Failure<(long, int, ProviderCredential)>(identityReference.Error);

        var added = await credentialStore.AddAsync(
            credentialScope,
            CredentialKind.EImzoCertificate,
            identityReference.Value,
            keyVersion: 1,
            validFromUtc: DateTime.UtcNow,
            expiresAtUtc: null,
            createdByUserId: null,
            ct);

        if (!added.IsSuccess)
            return Result.Failure<(long, int, ProviderCredential)>(added.Error);

        return Result.Success((added.Value.Id, added.Value.KeyVersion, added.Value));
    }

    private async Task<Result<OrganizationScope>> ResolveEdocsSessionScopeAsync(CancellationToken ct)
    {
        var baseScope = await scopeResolver.ResolveAsync(Provider.EDocs, ct: ct);
        if (!baseScope.IsSuccess)
            return Result.Failure<OrganizationScope>(baseScope.Error);

        var entities = await credentialStore.GetActiveEntityIdsAsync(
            baseScope.Value with { EntityId = null },
            CredentialKind.EImzoCertificate,
            ct);
        if (!entities.IsSuccess)
            return Result.Failure<OrganizationScope>(entities.Error);

        if (entities.Value.Count == 0)
            return Result.Failure<OrganizationScope>(ConnectionRequired);

        if (entities.Value.Count != 1)
            return Result.Failure<OrganizationScope>(Error.Conflict(
                "Edocs.CertificateSelectionRequired",
                "More than one E-DOCS certificate is active for the current organization."));

        return Result.Success(baseScope.Value with
        {
            EntityId = ProviderScopeCanonicalizer.NormalizeEdocsEntityId(entities.Value.Single())
        });
    }

    // Loads the active session (credential-version checked by the store) and opens its token for the
    // exact scope. Any missing/expired/revoked/version-mismatched/malformed session fails closed as
    // "reconnect required" — no plaintext token is ever surfaced.
    private async Task<Result<UsableSession>> LoadUsableSessionAsync(OrganizationScope scope, CancellationToken ct)
    {
        var session = await sessionStore.GetActiveAsync(scope, ct);
        if (!session.IsSuccess)
            return Result.Failure<UsableSession>(session.Error);

        if (session.Value is null)
            return Result.Failure<UsableSession>(ConnectionRequired);

        // Defense in depth on top of the store's own expiry filter: an expired session is
        // rejected here without unprotecting the token or calling the provider.
        if (session.Value.AccessExpiresAtUtc is { } expiresAt && expiresAt <= DateTime.UtcNow)
            return Result.Failure<UsableSession>(ConnectionRequired);

        var reference = EncryptedSecretReference.Create(session.Value.EncryptedAccessTokenReference);
        if (!reference.IsSuccess)
            return Result.Failure<UsableSession>(ConnectionRequired);

        var unprotected = secretProtector.Unprotect(scope, reference.Value);
        if (!unprotected.IsSuccess)
            return Result.Failure<UsableSession>(ConnectionRequired);

        return Result.Success(new UsableSession(unprotected.Value));
    }

    private async Task RevokeOnUnauthorizedAsync(Error error, OrganizationScope scope, string token, CancellationToken ct)
    {
        if (error.Type != ErrorType.Unauthorized)
            return;

        await sessionStore.RemoveIfMatchesAsync(scope, Fingerprint(token), ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    private static readonly Error ConnectionRequired =
        Error.Unauthorized("Edocs.ConnectionRequired", "Connect E-DOCS for the current organization first.");

    private static readonly Error LoginConcurrencyConflict =
        Error.Conflict("Edocs.LoginConcurrencyConflict", "E-DOCS connection changed concurrently. Please try again.");

    private static string CreateChallengeId() => Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();

    private static string Fingerprint(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))).ToLowerInvariant();

    private static string HashValue(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private static bool SameTin(string left, string right) =>
        CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(NormalizeTin(left)), Encoding.UTF8.GetBytes(NormalizeTin(right)));

    private static string NormalizeTin(string tin) => new(tin.Where(char.IsDigit).ToArray());

    private readonly record struct UsableSession(UnprotectedSecret Secret);
}
