using Application.Features.Edocs;
using SharedKernel.Results;

namespace Application.Abstractions.Integration;

public sealed record EdocsLoginResult(string Token, IReadOnlyCollection<string> EntityTins);

public sealed record EdocsExternalProfile(string Tin, string? Name);

public sealed record EdocsExternalDocument(string? Id, string? Number, string? Type, string? Status, DateTimeOffset? Date);

public sealed record EdocsExternalDocumentPage(IReadOnlyCollection<EdocsExternalDocument> Items, int? Total);

public sealed record EdocsChallengeRecord(string ChallengeId, int OrganizationId, string Tin, string SerialNumber, string AuthIdHash, DateTimeOffset ExpiresAtUtc);

public interface IEdocsClient
{
    Task<Result<string>> GetAuthIdAsync(string serialNumber, CancellationToken ct = default);
    Task<Result<EdocsLoginResult>> LoginAsync(string serialNumber, string pkcs7, CancellationToken ct = default);
    Task<Result<EdocsExternalProfile>> GetProfileAsync(string bearerToken, CancellationToken ct = default);
    Task<Result<EdocsExternalDocumentPage>> GetDocumentsAsync(string bearerToken, EdocsDocumentListQuery query, CancellationToken ct = default);
}

public interface IEdocsChallengeStore
{
    TimeSpan ChallengeTtl { get; }
    Task StoreAsync(EdocsChallengeRecord challenge, TimeSpan ttl, CancellationToken ct = default);
    Task<EdocsChallengeRecord?> ConsumeAsync(string challengeId, OrganizationScope scope, string serialNumber, CancellationToken ct = default);
}
