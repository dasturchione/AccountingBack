namespace Application.Abstractions.Integration.Edo;

public interface IEdoInboxService
{
    Task<EdoInboxListDto> ListInboxAsync(
        EdoInboxQueryDto request,
        CancellationToken ct = default);

    Task<EdoInboxListDto> ListDocumentsAsync(
        EdoDocumentQueryDto request,
        CancellationToken ct = default);

    Task<EdoInboxListDto> ListAllDocumentsAsync(
        EdoAllDocumentsQueryDto request,
        CancellationToken ct = default);

    Task<EdoDocumentDto> GetDetailsAsync(
        long id,
        CancellationToken ct = default);

    Task<EdoInboxSummaryDto> GetSummaryAsync(CancellationToken ct = default);

    Task<EdoInboxRejectDto> RejectAsync(
        long id,
        EdoInboxRejectRequestDto request,
        CancellationToken ct = default);

    Task<EdoFileDto> GetFileAsync(
        long id,
        CancellationToken ct = default);

    Task<EdoDocumentStatusDto> GetStatusAsync(
        long id,
        EdoDirection direction,
        CancellationToken ct = default);

    Task<EdoProviderDocumentStatusResponseDto> GetRemoteOutboxStatusAsync(
        string providerDocumentId,
        CancellationToken ct = default);
}
