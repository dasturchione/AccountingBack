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

    Task<EdoInboxListDto> ListInboxAsync(
        EdoInboxQueryDto request,
        CancellationToken ct = default);

    Task<EdoInboxRejectDto> RejectInboxAsync(
        string providerDocumentId,
        EdoInboxRejectRequestDto request,
        CancellationToken ct = default);

    Task<EdoFileDto> GetFileAsync(
        string providerDocumentId,
        CancellationToken ct = default);

    Task<EdoDocumentStatusDto> GetOutboxStatusAsync(
        string providerDocumentId,
        CancellationToken ct = default);

    Task<EdoDocumentStatusDto> GetInboxStatusAsync(
        string providerDocumentId,
        CancellationToken ct = default);
}
