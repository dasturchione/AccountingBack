using System.Linq;
using System.Text.Json.Nodes;
using Application.Features.Cmn.AslBelgi.Abstractions;
using Application.Features.Cmn.AslBelgi.DTOs;
using Application.Features.Cmn.AslBelgi.Errors;
using Application.Abstractions.Integration;
using Application.Features.Integration;
using Domain.Entities;
using SharedKernel.Results;
using SharedKernel.Exceptions;

namespace Application.Features.Cmn.AslBelgi.Services;

public sealed class AslBelgiService : IAslBelgiService
{
    private readonly IAslBelgiClient _client;
    private readonly IAslBelgiTokenProvider _tokenProvider;
    private readonly IOrganizationScopeResolver _scopeResolver;
    private readonly IAslBelgiOrganizationCapabilityResolver _capabilityResolver;

    public AslBelgiService(
        IAslBelgiClient client,
        IAslBelgiTokenProvider tokenProvider,
        IOrganizationScopeResolver scopeResolver,
        IAslBelgiOrganizationCapabilityResolver capabilityResolver)
    {
        _client = client;
        _tokenProvider = tokenProvider;
        _scopeResolver = scopeResolver;
        _capabilityResolver = capabilityResolver;
    }

    public async Task<Result<AslBelgiCheckApiKeyResponseDto>> CheckApiKeyAsync(string tin, CancellationToken ct = default)
    {
        var scope = await ResolveScopeAsync(ct);
        if (!scope.IsSuccess)
            return Result.Failure<AslBelgiCheckApiKeyResponseDto>(scope.Error);

        try
        {
            var tokenResult = await _tokenProvider.GetAccessTokenAsync(ct);
            if (!tokenResult.IsSuccess)
                return Result.Failure<AslBelgiCheckApiKeyResponseDto>(tokenResult.Error);

            var response = await _client.CheckApiKeyAsync(scope.Value.ExternalTin, tokenResult.Value, ct);
            return MapIntegrationResult(response, response => new AslBelgiCheckApiKeyResponseDto
            {
                IsValid = response.IsValid,
                ExpiresOn = response.ExpiresOn
            });
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return Result.Failure<AslBelgiCheckApiKeyResponseDto>(SafeProviderError);
        }
    }

    public async Task<Result<AslBelgiOrderResponse>> RegisterOrderAsync(AslBelgiOrderRequest request, CancellationToken ct = default)
    {
        var writeGate = ProviderWriteGate.RequireContract(Provider.AslBelgi);
        if (!writeGate.IsSuccess)
            return Result.Failure<AslBelgiOrderResponse>(writeGate.Error);

        var scope = await ResolveScopeAsync(ct);
        if (!scope.IsSuccess)
            return Result.Failure<AslBelgiOrderResponse>(scope.Error);

        if (!IsEmitter(scope.Value))
            return Result.Failure<AslBelgiOrderResponse>(AslBelgiErrors.EmitterRequired());

        if (request is null)
            return Result.Failure<AslBelgiOrderResponse>(AslBelgiErrors.MissingOrderPayload());

        if (request.Products is null || request.Products.Count == 0)
            return Result.Failure<AslBelgiOrderResponse>(AslBelgiErrors.MissingOrderProducts());

        if (request.Products.Any(p => string.IsNullOrWhiteSpace(p.Gtin)))
            return Result.Failure<AslBelgiOrderResponse>(AslBelgiErrors.MissingGtin());

        try
        {
            var tokenResult = await _tokenProvider.GetAccessTokenAsync(ct);
            if (!tokenResult.IsSuccess)
                return Result.Failure<AslBelgiOrderResponse>(tokenResult.Error);

            var response = await _client.RegisterOrderAsync(request, tokenResult.Value, ct);
            if (string.IsNullOrWhiteSpace(response.OrderId))
                return Result.Failure<AslBelgiOrderResponse>(AslBelgiErrors.IntegrationReturnedNoOrderId());

            return Result.Success(response);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return Result.Failure<AslBelgiOrderResponse>(SafeProviderError);
        }
    }

    public async Task<Result<IReadOnlyList<AslBelgiOrderInfo>>> GetOrdersAsync(AslBelgiOrdersFilter filter, CancellationToken ct = default)
    {
        var scope = await ResolveScopeAsync(ct);
        if (!scope.IsSuccess)
            return Result.Failure<IReadOnlyList<AslBelgiOrderInfo>>(scope.Error);

        if (!IsEmitter(scope.Value))
            return Result.Failure<IReadOnlyList<AslBelgiOrderInfo>>(AslBelgiErrors.EmitterRequired());

        try
        {
            var tokenResult = await _tokenProvider.GetAccessTokenAsync(ct);
            if (!tokenResult.IsSuccess)
                return Result.Failure<IReadOnlyList<AslBelgiOrderInfo>>(tokenResult.Error);
            return Result.Success(await _client.GetOrdersAsync(filter ?? new AslBelgiOrdersFilter(), tokenResult.Value, ct));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return Result.Failure<IReadOnlyList<AslBelgiOrderInfo>>(SafeProviderError);
        }
    }

    public async Task<Result<AslBelgiCodesResponse>> GetCodesAsync(string orderId, string? gtin, int? quantity, string? lastPackId, CancellationToken ct = default)
    {
        // Although this maps to a provider GET, quantity/lastPackId suggest sequential code
        // issuance (possible provider-side consume). Gated like a write until the contract
        // confirms the call is side-effect free.
        var writeGate = ProviderWriteGate.RequireContract(Provider.AslBelgi);
        if (!writeGate.IsSuccess)
            return Result.Failure<AslBelgiCodesResponse>(writeGate.Error);

        var scope = await ResolveScopeAsync(ct);
        if (!scope.IsSuccess)
            return Result.Failure<AslBelgiCodesResponse>(scope.Error);

        if (!IsEmitter(scope.Value))
            return Result.Failure<AslBelgiCodesResponse>(AslBelgiErrors.EmitterRequired());

        if (string.IsNullOrWhiteSpace(orderId))
            return Result.Failure<AslBelgiCodesResponse>(AslBelgiErrors.MissingOrderId());

        try
        {
            var tokenResult = await _tokenProvider.GetAccessTokenAsync(ct);
            if (!tokenResult.IsSuccess)
                return Result.Failure<AslBelgiCodesResponse>(tokenResult.Error);
            return Result.Success(await _client.GetCodesAsync(orderId, gtin, quantity, lastPackId, tokenResult.Value, ct));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return Result.Failure<AslBelgiCodesResponse>(SafeProviderError);
        }
    }

    public async Task<Result<AslBelgiDocumentResponseDto>> GetDocumentAsync(string documentId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(documentId))
            return Result.Failure<AslBelgiDocumentResponseDto>(AslBelgiErrors.MissingTin());

        var scope = await ResolveScopeAsync(ct);
        if (!scope.IsSuccess)
            return Result.Failure<AslBelgiDocumentResponseDto>(scope.Error);

        try
        {
            var tokenResult = await _tokenProvider.GetAccessTokenAsync(ct);
            if (!tokenResult.IsSuccess)
                return Result.Failure<AslBelgiDocumentResponseDto>(tokenResult.Error);
            var response = await _client.GetDocumentAsync(documentId, tokenResult.Value, ct);
            return MapIntegrationResult(response, response => new AslBelgiDocumentResponseDto { DocumentId = response.DocumentId, Content = response.Content });
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return Result.Failure<AslBelgiDocumentResponseDto>(SafeProviderError);
        }
    }

    public async Task<Result<AslBelgiStatusResponseDto>> GetStatusAsync(string identifier, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            return Result.Failure<AslBelgiStatusResponseDto>(AslBelgiErrors.MissingTin());

        var scope = await ResolveScopeAsync(ct);
        if (!scope.IsSuccess)
            return Result.Failure<AslBelgiStatusResponseDto>(scope.Error);

        try
        {
            var tokenResult = await _tokenProvider.GetAccessTokenAsync(ct);
            if (!tokenResult.IsSuccess)
                return Result.Failure<AslBelgiStatusResponseDto>(tokenResult.Error);
            var response = await _client.GetStatusAsync(identifier, tokenResult.Value, ct);
            return MapIntegrationResult(response, response => new AslBelgiStatusResponseDto { Id = response.Id, Details = response.Details });
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return Result.Failure<AslBelgiStatusResponseDto>(SafeProviderError);
        }
    }

    public async Task<Result<AslBelgiRefreshApiKeyResponseDto>> RefreshApiKeyAsync(
        AslBelgiRefreshApiKeyRequestDto request,
        CancellationToken ct = default)
    {
        var writeGate = ProviderWriteGate.RequireContract(Provider.AslBelgi);
        if (!writeGate.IsSuccess)
            return Result.Failure<AslBelgiRefreshApiKeyResponseDto>(writeGate.Error);

        var scope = await ResolveScopeAsync(ct);
        if (!scope.IsSuccess)
            return Result.Failure<AslBelgiRefreshApiKeyResponseDto>(scope.Error);

        if (string.IsNullOrWhiteSpace(request?.Id))
            return Result.Failure<AslBelgiRefreshApiKeyResponseDto>(AslBelgiErrors.MissingRefreshIdentifier());

        try
        {
            var tokenResult = await _tokenProvider.GetAccessTokenAsync(ct);
            if (!tokenResult.IsSuccess)
                return Result.Failure<AslBelgiRefreshApiKeyResponseDto>(tokenResult.Error);

            var response = await _client.RefreshApiKeyAsync(
                scope.Value.ExternalTin,
                tokenResult.Value,
                new AslBelgiRefreshApiKeyRequestDto
                {
                    Tin = scope.Value.ExternalTin,
                    ApiKey = null,
                    Id = request.Id
                },
                ct);

            return MapIntegrationResult(response, response => new AslBelgiRefreshApiKeyResponseDto
            {
                ApiKey = null,
                Id = response.Id,
                ExpiresOn = response.ExpiresOn,
                Label = response.Label
            });
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return Result.Failure<AslBelgiRefreshApiKeyResponseDto>(SafeProviderError);
        }
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
            return Result.Failure<TResponseDto>(SafeProviderError);

        var dto = mapPayload(response);
        dto.Status = mapped.Status;
        dto.Message = null;
        dto.Errors = null;
        dto.Data = null;
        dto.Items = null;
        dto.Envelope = null;

        return Result.Success(dto);
    }

    private static Error? CheckForIntegrationFailure(EnvelopeSummary envelope)
    {
        if (envelope.Errors is not null && envelope.Errors.Length > 0)
            return SafeProviderError;

        if (string.IsNullOrWhiteSpace(envelope.Status))
            return null;

        if (IsSuccessStatus(envelope.Status))
            return null;

        if (string.Equals(envelope.Status, "error", StringComparison.OrdinalIgnoreCase)
            || string.Equals(envelope.Status, "failed", StringComparison.OrdinalIgnoreCase)
            || string.Equals(envelope.Status, "fail", StringComparison.OrdinalIgnoreCase))
            return SafeProviderError;

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

    private async Task<Result<OrganizationScope>> ResolveScopeAsync(CancellationToken ct)
    {
        var scope = await _scopeResolver.ResolveAsync(Provider.AslBelgi, ct: ct);
        return scope.IsSuccess ? scope : Result.Failure<OrganizationScope>(scope.Error);
    }

    private bool IsEmitter(OrganizationScope scope) =>
        _capabilityResolver.Resolve(scope.OrganizationId) == AslBelgiOrganizationCapability.Emitter;

    private static readonly Error SafeProviderError = Error.Problem(
        "AslBelgi.ProviderError", "Asl Belgisi request failed.");
}
