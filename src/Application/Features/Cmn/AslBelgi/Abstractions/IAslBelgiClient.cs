using Application.Features.Cmn.AslBelgi.DTOs;

namespace Application.Features.Cmn.AslBelgi.Abstractions;

public interface IAslBelgiClient
{
    Task<AslBelgiCheckApiKeyResponseDto> CheckApiKeyAsync(string tin, string accessToken, CancellationToken ct = default);

    Task<AslBelgiOrderResponse> RegisterOrderAsync(AslBelgiOrderRequest request, string accessToken, CancellationToken ct = default);

    Task<IReadOnlyList<AslBelgiOrderInfo>> GetOrdersAsync(AslBelgiOrdersFilter filter, string accessToken, CancellationToken ct = default);

    Task<AslBelgiCodesResponse> GetCodesAsync(string orderId, string? gtin, int? quantity, string? lastPackId, string accessToken, CancellationToken ct = default);

    Task<AslBelgiDocumentResponseDto> GetDocumentAsync(string documentId, string accessToken, CancellationToken ct = default);

    Task<AslBelgiStatusResponseDto> GetStatusAsync(string identifier, string accessToken, CancellationToken ct = default);

    Task<AslBelgiRefreshApiKeyResponseDto> RefreshApiKeyAsync(
        string tin,
        string accessToken,
        AslBelgiRefreshApiKeyRequestDto request,
        CancellationToken ct = default);
}
