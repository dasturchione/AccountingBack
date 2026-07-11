using Application.Features.Cmn.AslBelgi.DTOs;
using SharedKernel.Results;

namespace Application.Features.Cmn.AslBelgi.Abstractions;

public interface IAslBelgiService
{
    Task<Result<AslBelgiCheckApiKeyResponseDto>> CheckApiKeyAsync(string tin, CancellationToken ct = default);

    Task<Result<AslBelgiOrderResponse>> RegisterOrderAsync(AslBelgiOrderRequest request, CancellationToken ct = default);

    Task<Result<IReadOnlyList<AslBelgiOrderInfo>>> GetOrdersAsync(AslBelgiOrdersFilter filter, CancellationToken ct = default);

    Task<Result<AslBelgiCodesResponse>> GetCodesAsync(string orderId, string? gtin, int? quantity, string? lastPackId, CancellationToken ct = default);

    Task<Result<AslBelgiDocumentResponseDto>> GetDocumentAsync(string documentId, CancellationToken ct = default);

    Task<Result<AslBelgiStatusResponseDto>> GetStatusAsync(string identifier, CancellationToken ct = default);

    Task<Result<AslBelgiRefreshApiKeyResponseDto>> RefreshApiKeyAsync(
        AslBelgiRefreshApiKeyRequestDto request,
        CancellationToken ct = default);
}
