using System.Linq;
using System.Text.Json.Nodes;
using Application.Features.Cmn.AslBelgi.Abstractions;
using Application.Features.Cmn.AslBelgi.DTOs;
using Application.Features.Cmn.AslBelgi.Errors;
using SharedKernel.Results;
 

namespace Application.Features.Cmn.AslBelgi.Services;

public sealed class AslBelgiService : IAslBelgiService
{
    private readonly IAslBelgiClient _client;
    private readonly IAslBelgiTokenProvider _tokenProvider;

    public AslBelgiService(IAslBelgiClient client, IAslBelgiTokenProvider tokenProvider)
    {
        _client = client;
        _tokenProvider = tokenProvider;
    }

    public async Task<Result<AslBelgiCheckApiKeyResponseDto>> CheckApiKeyAsync(string tin, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(tin))
            return Result.Failure<AslBelgiCheckApiKeyResponseDto>(AslBelgiErrors.MissingTin());

        var tokenResult = await _tokenProvider.GetAccessTokenAsync(ct);
        if (!tokenResult.IsSuccess)
            return Result.Failure<AslBelgiCheckApiKeyResponseDto>(tokenResult.Error);

        var response = await _client.CheckApiKeyAsync(tin, tokenResult.Value, ct);
        return MapIntegrationResult(
            response,
            response => new AslBelgiCheckApiKeyResponseDto
            {
                IsValid = response.IsValid,
                ApiKey = response.ApiKey
            });
    }

    public async Task<Result<AslBelgiOrderResponse>> RegisterOrderAsync(AslBelgiOrderRequest request, CancellationToken ct = default)
    {
        if (request is null)
            return Result.Failure<AslBelgiOrderResponse>(AslBelgiErrors.MissingOrderPayload());

        if (request.Products is null || request.Products.Count == 0)
            return Result.Failure<AslBelgiOrderResponse>(AslBelgiErrors.MissingOrderProducts());

        if (request.Products.Any(p => string.IsNullOrWhiteSpace(p.Gtin)))
            return Result.Failure<AslBelgiOrderResponse>(AslBelgiErrors.MissingGtin());

        var tokenResult = await _tokenProvider.GetAccessTokenAsync(ct);
        if (!tokenResult.IsSuccess)
            return Result.Failure<AslBelgiOrderResponse>(tokenResult.Error);

        var response = await _client.RegisterOrderAsync(request, tokenResult.Value, ct);
        if (string.IsNullOrWhiteSpace(response.OrderId))
            return Result.Failure<AslBelgiOrderResponse>(AslBelgiErrors.IntegrationReturnedNoOrderId());

        return Result.Success(response);
    }

    public async Task<Result<IReadOnlyList<AslBelgiOrderInfo>>> GetOrdersAsync(AslBelgiOrdersFilter filter, CancellationToken ct = default)
    {
        var tokenResult = await _tokenProvider.GetAccessTokenAsync(ct);
        if (!tokenResult.IsSuccess)
            return Result.Failure<IReadOnlyList<AslBelgiOrderInfo>>(tokenResult.Error);

        var response = await _client.GetOrdersAsync(filter ?? new AslBelgiOrdersFilter(), tokenResult.Value, ct);
        return Result.Success(response);
    }

    public async Task<Result<AslBelgiCodesResponse>> GetCodesAsync(string orderId, string? gtin, int? quantity, string? lastPackId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(orderId))
            return Result.Failure<AslBelgiCodesResponse>(AslBelgiErrors.MissingOrderId());

        var tokenResult = await _tokenProvider.GetAccessTokenAsync(ct);
        if (!tokenResult.IsSuccess)
            return Result.Failure<AslBelgiCodesResponse>(tokenResult.Error);

        var response = await _client.GetCodesAsync(orderId, gtin, quantity, lastPackId, tokenResult.Value, ct);
        return Result.Success(response);
    }

    public async Task<Result<AslBelgiDocumentResponseDto>> GetDocumentAsync(string documentId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(documentId))
            return Result.Failure<AslBelgiDocumentResponseDto>(AslBelgiErrors.MissingTin());

        var tokenResult = await _tokenProvider.GetAccessTokenAsync(ct);
        if (!tokenResult.IsSuccess)
            return Result.Failure<AslBelgiDocumentResponseDto>(tokenResult.Error);

        var response = await _client.GetDocumentAsync(documentId, tokenResult.Value, ct);
        return MapIntegrationResult(
            response,
            response => new AslBelgiDocumentResponseDto
            {
                DocumentId = response.DocumentId,
                Content = response.Content
            });
    }

    public async Task<Result<AslBelgiStatusResponseDto>> GetStatusAsync(string identifier, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            return Result.Failure<AslBelgiStatusResponseDto>(AslBelgiErrors.MissingTin());

        var tokenResult = await _tokenProvider.GetAccessTokenAsync(ct);
        if (!tokenResult.IsSuccess)
            return Result.Failure<AslBelgiStatusResponseDto>(tokenResult.Error);

        var response = await _client.GetStatusAsync(identifier, tokenResult.Value, ct);
        return MapIntegrationResult(
            response,
            response => new AslBelgiStatusResponseDto
            {
                Id = response.Id,
                Details = response.Details
            });
    }

    public async Task<Result<AslBelgiRefreshApiKeyResponseDto>> RefreshApiKeyAsync(
        AslBelgiRefreshApiKeyRequestDto request,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request?.Tin))
            return Result.Failure<AslBelgiRefreshApiKeyResponseDto>(AslBelgiErrors.MissingTin());

        if (string.IsNullOrWhiteSpace(request.ApiKey) && string.IsNullOrWhiteSpace(request.Id))
            return Result.Failure<AslBelgiRefreshApiKeyResponseDto>(AslBelgiErrors.MissingRefreshIdentifier());

        var tokenResult = await _tokenProvider.GetAccessTokenAsync(ct);
        if (!tokenResult.IsSuccess)
            return Result.Failure<AslBelgiRefreshApiKeyResponseDto>(tokenResult.Error);

        var response = await _client.RefreshApiKeyAsync(
            request.Tin!,
            tokenResult.Value,
            new AslBelgiRefreshApiKeyRequestDto
            {
                Tin = request.Tin,
                ApiKey = request.ApiKey,
                Id = request.Id
            },
            ct);

        return MapIntegrationResult(
            response,
            response => new AslBelgiRefreshApiKeyResponseDto
            {
                ApiKey = response.ApiKey,
                Id = response.Id,
                ExpiresOn = response.ExpiresOn,
                Label = response.Label
            });
    }

    private static Result<TResponseDto> MapIntegrationResult<TResponse, TResponseDto>(
        TResponse response,
        Func<TResponse, TResponseDto> mapPayload)
        where TResponse : AslBelgiResultDto
        where TResponseDto : AslBelgiResultDto
    {
        var mapped = ToEnvelope(response);
        var failure = CheckForIntegrationFailure(mapped);

        if (failure is not null)
        {
            return Result.Failure<TResponseDto>(failure);
        }

        var dto = mapPayload(response);
        dto.Status = mapped.Status;
        dto.Message = mapped.Message;
        dto.Errors = mapped.Errors;
        dto.Data = mapped.Data;
        dto.Items = mapped.Items;
        dto.Envelope = new AslBelgiEnvelopeDto
        {
            Status = mapped.Status,
            Message = mapped.Message,
            Errors = mapped.Errors,
            Data = mapped.Data,
            Items = mapped.Items
        };

        return Result.Success(dto);
    }

    private static Error? CheckForIntegrationFailure(EnvelopeSummary envelope)
    {
        if (envelope.Errors is not null && envelope.Errors.Length > 0)
            return AslBelgiErrors.IntegrationReturnedError("Asl Belgisi returned business errors.", envelope.Message);

        if (string.IsNullOrWhiteSpace(envelope.Status))
            return null;

        if (IsSuccessStatus(envelope.Status))
            return null;

        if (string.Equals(envelope.Status, "error", StringComparison.OrdinalIgnoreCase)
            || string.Equals(envelope.Status, "failed", StringComparison.OrdinalIgnoreCase)
            || string.Equals(envelope.Status, "fail", StringComparison.OrdinalIgnoreCase))
            return AslBelgiErrors.IntegrationReturnedError("Asl Belgisi returned business status.", envelope.Message);

        return null;
    }

    private static bool IsSuccessStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return false;

        return string.Equals(status, "success", StringComparison.OrdinalIgnoreCase)
            || string.Equals(status, "ok", StringComparison.OrdinalIgnoreCase);
    }

    private static EnvelopeSummary ToEnvelope(AslBelgiResultDto response)
    {
        var status = response.Status ?? response.Envelope?.Status;
        var message = response.Message ?? response.Envelope?.Message;
        var errors = response.Errors ?? response.Envelope?.Errors;
        var data = response.Envelope?.Data;
        var items = response.Envelope?.Items;

        return new EnvelopeSummary
        {
            Status = status,
            Message = message,
            Errors = errors,
            Data = data,
            Items = items
        };
    }

    private sealed class EnvelopeSummary
    {
        public string? Status { get; set; }
        public string? Message { get; set; }
        public string[]? Errors { get; set; }
        public JsonNode? Data { get; set; }
        public JsonArray? Items { get; set; }
    }
}
