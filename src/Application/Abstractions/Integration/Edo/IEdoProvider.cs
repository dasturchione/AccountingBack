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

    Task<EdoDocumentStatusDto> GetInboxStatusAsync(
        string providerDocumentId,
        CancellationToken ct = default);
}
