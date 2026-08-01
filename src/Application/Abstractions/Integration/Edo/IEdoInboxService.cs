namespace Application.Abstractions.Integration.Edo;

public interface IEdoInboxService
{
    Task<EdoInboxListDto> ListInboxAsync(
        EdoInboxQueryDto request,
        CancellationToken ct = default);

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
}
