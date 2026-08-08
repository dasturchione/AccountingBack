using SharedKernel.Exceptions;

namespace Application.Abstractions.Integration.Edo;

public interface IEdoProvider
{
    EdoProviderCode Code { get; }
    EdoProviderCapabilityDto Capabilities { get; }

    Task<EdoAuthChallengeDto> GetAuthChallengeAsync(
        EdoAuthChallengeRequestDto request,
        CancellationToken ct = default);

    Task<EdoAuthCompleteDto> CompleteAuthAsync(
        EdoAuthCompleteRequestDto request,
        CancellationToken ct = default);

    Task<EdoOutboxCreateDto> CreateFacturaAsync(
        EdoOutboxFacturaCreateRequestDto request,
        CancellationToken ct = default);

    Task<EdoOutboxSignDto> SignOutboxAsync(
        long id,
        EdoOutboxSignRequestDto request,
        CancellationToken ct = default);

    Task<EdoOutboxSignDto> SignOutboxAsync(
        string providerDocumentId,
        EdoOutboxSignRequestDto request,
        CancellationToken ct = default) =>
        throw new NotSupportedException("This EDO provider does not expose string document-id signing.");

    Task<EdoInboxListDto> ListInboxAsync(
        EdoInboxQueryDto request,
        CancellationToken ct = default);

    Task<EdoInboxListDto> ListDocumentsAsync(
        EdoDocumentQueryDto request,
        CancellationToken ct = default)
    {
        if (request.Scope == EdoDocumentQueryScope.INBOX)
            return ListInboxAsync(request.ToInboxQuery(), ct);

        throw new EdoCapabilityUnavailableException(
            Code.ToString(),
            request.Scope == EdoDocumentQueryScope.OUTBOX
                ? EdoCapabilityKind.ListOutbox.ToString()
                : EdoCapabilityKind.ListAll.ToString(),
            EdoCapabilityStatus.UNKNOWN.ToString());
    }

    Task<EdoDocumentDto> GetDocumentDetailsAsync(
        EdoDirection direction,
        string providerDocumentType,
        string providerDocumentId,
        CancellationToken ct = default) =>
        throw new EdoCapabilityUnavailableException(
            Code.ToString(),
            EdoCapabilityKind.GetDetail.ToString(),
            EdoCapabilityStatus.UNKNOWN.ToString());

    Task<EdoInboxSummaryDto> GetInboxSummaryAsync(CancellationToken ct = default) =>
        throw new NotSupportedException("This EDO provider does not expose document summary statistics.");

    Task<EdoInboxRejectDto> RejectInboxAsync(
        string providerDocumentType,
        string providerDocumentId,
        EdoInboxRejectRequestDto request,
        CancellationToken ct = default);

    Task<EdoFileDto> GetFileAsync(
        string providerDocumentType,
        string providerDocumentId,
        CancellationToken ct = default);

    Task<EdoDocumentStatusDto> GetOutboxStatusAsync(
        string providerDocumentId,
        CancellationToken ct = default);

    Task<EdoDocumentStatusDto> GetOutboxStatusAsync(
        string providerDocumentType,
        string providerDocumentId,
        CancellationToken ct = default) =>
        GetOutboxStatusAsync(providerDocumentId, ct);

    Task<EdoDocumentStatusDto> GetInboxStatusAsync(
        string providerDocumentId,
        CancellationToken ct = default);

    Task<EdoDocumentStatusDto> GetInboxStatusAsync(
        string providerDocumentType,
        string providerDocumentId,
        CancellationToken ct = default) =>
        GetInboxStatusAsync(providerDocumentId, ct);
}
