using Application.Features.Integration.AslBelgi.DTOs;
using System.Text.Json;

namespace Application.Features.Integration.AslBelgi.Transfers;

public sealed class CrptTransferRequestSubmitDto
{
    public int SellerCounterpartyId { get; init; }
    public int BuyerCounterpartyId { get; init; }
    public string IdempotencyKey { get; init; } = string.Empty;
    public CrptSignedDocumentPayloadDto SignedPayload { get; init; } = new();
}

public sealed class CrptTransferConfirmationSubmitDto
{
    public string IdempotencyKey { get; init; } = string.Empty;
    public CrptSignedDocumentPayloadDto SignedPayload { get; init; } = new();
}

public sealed class CrptTransferWriteResultDto
{
    public string DocumentId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public bool IsReplay { get; init; }
}

public sealed class CrptTransferSearchRequestDto
{
    public string? DocumentId { get; init; }
    public IReadOnlyCollection<string>? Statuses { get; init; }
    public IReadOnlyCollection<string>? ConfirmationStatuses { get; init; }
    public int? OwnerId { get; init; }
    public int? BuyerId { get; init; }
    public DateTimeOffset? DateFrom { get; init; }
    public DateTimeOffset? DateTo { get; init; }
    public DateTimeOffset? BusinessDateFrom { get; init; }
    public DateTimeOffset? BusinessDateTo { get; init; }
    public int? Limit { get; init; }
    public string? Cursor { get; init; }
}

public interface IAslBelgiTransferService
{
    Task<JsonElement> SearchTransferRequestsAsync(CrptTransferSearchRequestDto request, CancellationToken ct = default);
    Task<JsonElement> GetDocumentStatusAsync(string documentId, CancellationToken ct = default);
    Task<CrptTransferWriteResultDto> SubmitTransferRequestAsync(CrptTransferRequestSubmitDto request, CancellationToken ct = default);
    Task<CrptTransferWriteResultDto> SubmitTransferConfirmationAsync(CrptTransferConfirmationSubmitDto request, CancellationToken ct = default);
}
