using Application.Features.Cmn.AslBelgi.Abstractions;
using Application.Features.Cmn.AslBelgi.DTOs;
using Integration.AslBelgi.Configs;
using Integration.AslBelgi.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel.Exceptions;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Integration.AslBelgi.Services;

public sealed class AslBelgiClient : AslBelgiHttpClientBase, IAslBelgiClient
{
    private readonly IMemoryCache _cache;
    private const string ClientName = "AslBelgi";
    private const string CheckApiKeyCachePrefix = "AslBelgi:ApiKeyCheck:";
    private const int InitialRetryDelayMs = 250;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public AslBelgiClient(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor,
        IOptions<AslBelgiSettings> settings,
        IMemoryCache cache,
        ILogger<AslBelgiClient> logger)
        : base(httpClientFactory, httpContextAccessor, settings, logger)
    {
        _cache = cache;
    }

    public async Task<AslBelgiCheckApiKeyResponseDto> CheckApiKeyAsync(string tin, string accessToken, CancellationToken ct = default)
    {
        // apiKey check is rate-limited (100/24h); cache the result per TIN to stay within the limit.
        var cacheKey = CheckApiKeyCachePrefix + tin;
        if (_cache.TryGetValue(cacheKey, out AslBelgiCheckApiKeyResponseDto? cached) && cached is not null)
            return cached;

        var path = ResolvePath(Settings.CheckApiKeyPath, "{tin}", tin);
        var response = await SendAsync<AslBelgiCheckApiKeyResponse>(HttpMethod.Get, path, null, accessToken, ct);
        var result = MapCheck(response);

        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(Math.Max(1, Settings.ApiKeyCheckCacheMinutes)));
        return result;
    }

    public async Task<AslBelgiOrderResponse> RegisterOrderAsync(AslBelgiOrderRequest request, string accessToken, CancellationToken ct = default)
    {
        var response = await SendJsonAsync<AslBelgiOrderResponse>(HttpMethod.Post, Settings.OrdersPath, request, accessToken, ct);
        return response ?? new AslBelgiOrderResponse();
    }

    public async Task<IReadOnlyList<AslBelgiOrderInfo>> GetOrdersAsync(AslBelgiOrdersFilter filter, string accessToken, CancellationToken ct = default)
    {
        var path = AppendQuery(Settings.OrdersPath,
        [
            new("orderId", filter.OrderId),
            new("status", filter.Status),
            new("productGroup", filter.ProductGroup),
            new("dateFrom", filter.DateFrom?.ToString("O")),
            new("dateTo", filter.DateTo?.ToString("O")),
            new("limit", filter.Limit?.ToString())
        ]);

        var response = await SendJsonAsync<List<AslBelgiOrderInfo>>(HttpMethod.Get, path, null, accessToken, ct);
        return response ?? [];
    }

    public async Task<AslBelgiCodesResponse> GetCodesAsync(string orderId, string? gtin, int? quantity, string? lastPackId, string accessToken, CancellationToken ct = default)
    {
        var path = AppendQuery(Settings.CodesPath,
        [
            new("orderId", orderId),
            new("gtin", gtin),
            new("quantity", quantity?.ToString()),
            new("lastPackId", lastPackId)
        ]);

        var response = await SendJsonAsync<AslBelgiCodesResponse>(HttpMethod.Get, path, null, accessToken, ct);
        return response ?? new AslBelgiCodesResponse();
    }

    public async Task<AslBelgiDocumentResponseDto> GetDocumentAsync(string documentId, string accessToken, CancellationToken ct = default)
    {
        var path = ResolvePath(Settings.DocumentsPath, "{documentId}", documentId);
        var response = await SendAsync<AslBelgiDocumentResponse>(HttpMethod.Get, path, null, accessToken, ct);
        return MapDocument(response);
    }

    public async Task<AslBelgiStatusResponseDto> GetStatusAsync(string identifier, string accessToken, CancellationToken ct = default)
    {
        var path = ResolvePath(Settings.StatusPath, "{id}", identifier);
        var response = await SendAsync<AslBelgiStatusResponse>(HttpMethod.Get, path, null, accessToken, ct);
        return MapStatus(response);
    }

    public async Task<AslBelgiRefreshApiKeyResponseDto> RefreshApiKeyAsync(
        string tin,
        string accessToken,
        AslBelgiRefreshApiKeyRequestDto request,
        CancellationToken ct = default)
    {
        var path = ResolvePath(Settings.RefreshApiKeyPath, "{tin}", tin);
        var response = await SendAsync<AslBelgiRefreshApiKeyResponse>(
            HttpMethod.Post,
            path,
            new { request.ApiKey, request.Id },
            accessToken,
            ct);

        return MapRefresh(response);
    }

    private async Task<TResponse> SendAsync<TResponse>(
        HttpMethod method,
        string path,
        object? body,
        string accessToken,
        CancellationToken ct)
    {
        var responseText = await SendRawAsync(method, path, body, accessToken, ct);
        return string.IsNullOrWhiteSpace(responseText)
            ? default!
            : DeserializeResponseOrEnvelope<TResponse>(responseText);
    }

    // Plain typed send for the order/codes endpoints — no legacy envelope unwrapping.
    private async Task<TResponse?> SendJsonAsync<TResponse>(
        HttpMethod method,
        string path,
        object? body,
        string accessToken,
        CancellationToken ct)
    {
        var responseText = await SendRawAsync(method, path, body, accessToken, ct);
        return string.IsNullOrWhiteSpace(responseText)
            ? default
            : JsonSerializer.Deserialize<TResponse>(responseText, JsonOptions);
    }

    private async Task<string> SendRawAsync(
        HttpMethod method,
        string path,
        object? body,
        string accessToken,
        CancellationToken ct)
    {
        var client = CreateClient(ClientName);

        using var request = new HttpRequestMessage(method, BuildUri(path))
        {
            Content = body is null
                ? null
                : new StringContent(
                    JsonSerializer.Serialize(body, JsonOptions),
                    Encoding.UTF8,
                    "application/json;charset=UTF-8")
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Accept.Clear();
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;charset=UTF-8"));
        ApplyCorrelationId(request);

        using var response = await SendWithRetryAsync(
            () => client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct),
            InitialRetryDelayMs,
            "request",
            ct);
        var responseText = await ReadBodySafelyAsync(response, ct);

        if (response.IsSuccessStatusCode)
            return responseText;

        var detail = BuildErrorMessage(responseText);
        throw CreateHttpException(response.StatusCode, detail);
    }

    private static string AppendQuery(string path, IReadOnlyList<KeyValuePair<string, string?>> query)
    {
        var pairs = query
            .Where(kv => !string.IsNullOrWhiteSpace(kv.Value))
            .Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value!)}");

        var queryString = string.Join("&", pairs);
        return string.IsNullOrEmpty(queryString) ? path : $"{path}?{queryString}";
    }

    private static TResponse DeserializeResponseOrEnvelope<TResponse>(string body)
    {
        var direct = TryDeserialize<TResponse>(body);
        if (direct is not null)
            return direct;

        var envelope = TryDeserialize<AslBelgiResponseEnvelope>(body);
        if (envelope is null)
            throw new IntegrationHttpException("Asl Belgisi response body is invalid: cannot deserialize response.", StatusCodes.Status502BadGateway);

        var payloadNode = envelope.Data ?? envelope.Items?.FirstOrDefault();
        if (payloadNode is null)
        {
            var fallback = CreateEnvelopeOnlyResponse<TResponse>(envelope);
            if (fallback is null)
                throw new IntegrationHttpException("Asl Belgisi response body is missing data/items payload.", StatusCodes.Status502BadGateway);

            ApplyEnvelopeMetadata(fallback, envelope);
            return fallback;
        }

        var response = TryDeserialize<TResponse>(payloadNode.ToJsonString());
        if (response is not null)
        {
            ApplyEnvelopeMetadata(response, envelope);
            return response;
        }

        var envelopeOnly = CreateEnvelopeOnlyResponse<TResponse>(envelope);
        if (envelopeOnly is null)
            throw new IntegrationHttpException("Asl Belgisi response body is invalid: cannot deserialize payload.", StatusCodes.Status502BadGateway);

        ApplyEnvelopeMetadata(envelopeOnly, envelope);
        return envelopeOnly;
    }

    private static bool IsSuccessStatus(string status)
    {
        return string.Equals(status, "success", StringComparison.OrdinalIgnoreCase)
            || string.Equals(status, "ok", StringComparison.OrdinalIgnoreCase);
    }

    private static void ApplyEnvelopeMetadata<TResponse>(TResponse response, AslBelgiResponseEnvelope envelope)
    {
        switch (response)
        {
            case AslBelgiCheckApiKeyResponse cast:
                cast.Status = cast.Status ?? envelope.Status;
                cast.Message = cast.Message ?? envelope.Message;
                cast.Errors = cast.Errors ?? envelope.Errors;
                cast.Envelope = envelope;
                break;
            case AslBelgiSubmitResponse cast:
                cast.Status = cast.Status ?? envelope.Status;
                cast.Message = cast.Message ?? envelope.Message;
                cast.Errors = cast.Errors ?? envelope.Errors;
                cast.Envelope = envelope;
                break;
            case AslBelgiDocumentResponse cast:
                cast.Status = cast.Status ?? envelope.Status;
                cast.Message = cast.Message ?? envelope.Message;
                cast.Errors = cast.Errors ?? envelope.Errors;
                cast.Envelope = envelope;
                break;
            case AslBelgiStatusResponse cast:
                cast.Status = cast.Status ?? envelope.Status;
                cast.Message = cast.Message ?? envelope.Message;
                cast.Errors = cast.Errors ?? envelope.Errors;
                cast.Envelope = envelope;
                break;
            case AslBelgiRefreshApiKeyResponse cast:
                cast.Status = cast.Status ?? envelope.Status;
                cast.Message = cast.Message ?? envelope.Message;
                cast.Errors = cast.Errors ?? envelope.Errors;
                cast.Envelope = envelope;
                break;
        }
    }

    private static TResponse? CreateEnvelopeOnlyResponse<TResponse>(AslBelgiResponseEnvelope envelope)
    {
        object? response = null;
        var targetType = typeof(TResponse);

        if (targetType == typeof(AslBelgiCheckApiKeyResponse))
            response = new AslBelgiCheckApiKeyResponse();
        else if (targetType == typeof(AslBelgiSubmitResponse))
            response = new AslBelgiSubmitResponse();
        else if (targetType == typeof(AslBelgiDocumentResponse))
            response = new AslBelgiDocumentResponse();
        else if (targetType == typeof(AslBelgiStatusResponse))
            response = new AslBelgiStatusResponse();
        else if (targetType == typeof(AslBelgiRefreshApiKeyResponse))
            response = new AslBelgiRefreshApiKeyResponse();

        if (response is null)
            return default;

        return (TResponse)response;
    }

    private static TResponse? TryDeserialize<TResponse>(string body)
    {
        try
        {
            return JsonSerializer.Deserialize<TResponse>(body, JsonOptions);
        }
        catch (JsonException)
        {
            return default;
        }
    }

    private static Exception CreateHttpException(HttpStatusCode statusCode, string detail)
    {
        return statusCode switch
        {
            HttpStatusCode.Unauthorized => new IntegrationUnauthorizedException("AslBelgi request rejected. " + detail),
            HttpStatusCode.Forbidden => new IntegrationForbiddenException("AslBelgi request forbidden. " + detail),
            _ => new IntegrationHttpException($"AslBelgi request failed with status {(int)statusCode}. {detail}", (int)statusCode)
        };
    }

    private static string ResolvePath(string template, string token, string value)
        => template.Replace(token, Uri.EscapeDataString(value));

    private static string BuildErrorMessage(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return "No response body from Asl Belgisi.";

        try
        {
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;
            if (TryGetString(root, "message", out var message))
                return message!;

            if (TryGetString(root, "error", out var error))
                return error!;

            if (TryGetString(root, "detail", out var detail))
                return detail!;

            if (TryGetString(root, "title", out var title))
                return title!;

            if (root.ValueKind == JsonValueKind.Array && root.EnumerateArray().MoveNext())
                return "Asl Belgisi returned a structured error response.";
        }
        catch
        {
            // fall through
        }

        return body.Length <= 300 ? body : "Asl Belgisi returned an unexpected error response.";
    }

    private static bool TryGetString(JsonElement root, string propertyName, out string? value)
    {
        if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty(propertyName, out var property)
            && property.ValueKind == JsonValueKind.String)
        {
            value = property.GetString();
            return true;
        }

        value = null;
        return false;
    }

    private static AslBelgiCheckApiKeyResponseDto MapCheck(AslBelgiCheckApiKeyResponse response)
    {
        return new AslBelgiCheckApiKeyResponseDto
        {
            Status = response.Status,
            Message = response.Message,
            Errors = response.Errors is null ? null : response.Errors.Select(item => item?.ToString()).Where(item => !string.IsNullOrWhiteSpace(item)).Select(item => item!).ToArray(),
            Data = response.Envelope?.Data,
            Items = response.Envelope?.Items,
            Envelope = new AslBelgiEnvelopeDto
            {
                Status = response.Status,
                Message = response.Message,
                Errors = response.Errors is null ? null : response.Errors.Select(item => item?.ToString()).Where(item => !string.IsNullOrWhiteSpace(item)).Select(item => item!).ToArray(),
                Data = response.Envelope?.Data,
                Items = response.Envelope?.Items
            },
            IsValid = response.IsValid,
            ApiKey = response.ApiKey
        };
    }

    private static AslBelgiDocumentResponseDto MapDocument(AslBelgiDocumentResponse response)
    {
        return new AslBelgiDocumentResponseDto
        {
            Status = response.Status,
            Message = response.Message,
            Errors = response.Errors is null ? null : response.Errors.Select(item => item?.ToString()).Where(item => !string.IsNullOrWhiteSpace(item)).Select(item => item!).ToArray(),
            Data = response.Envelope?.Data,
            Items = response.Envelope?.Items,
            Envelope = new AslBelgiEnvelopeDto
            {
                Status = response.Status,
                Message = response.Message,
                Errors = response.Errors is null ? null : response.Errors.Select(item => item?.ToString()).Where(item => !string.IsNullOrWhiteSpace(item)).Select(item => item!).ToArray(),
                Data = response.Envelope?.Data,
                Items = response.Envelope?.Items
            },
            DocumentId = response.DocumentId,
            Content = response.Content
        };
    }

    private static AslBelgiStatusResponseDto MapStatus(AslBelgiStatusResponse response)
    {
        return new AslBelgiStatusResponseDto
        {
            Status = response.Status,
            Message = response.Message,
            Errors = response.Errors is null ? null : response.Errors.Select(item => item?.ToString()).Where(item => !string.IsNullOrWhiteSpace(item)).Select(item => item!).ToArray(),
            Data = response.Envelope?.Data,
            Items = response.Envelope?.Items,
            Envelope = new AslBelgiEnvelopeDto
            {
                Status = response.Status,
                Message = response.Message,
                Errors = response.Errors is null ? null : response.Errors.Select(item => item?.ToString()).Where(item => !string.IsNullOrWhiteSpace(item)).Select(item => item!).ToArray(),
                Data = response.Envelope?.Data,
                Items = response.Envelope?.Items
            },
            Id = response.Id,
            Details = response.Details
        };
    }

    private static AslBelgiRefreshApiKeyResponseDto MapRefresh(AslBelgiRefreshApiKeyResponse response)
    {
        return new AslBelgiRefreshApiKeyResponseDto
        {
            Status = response.Status,
            Message = response.Message,
            Errors = response.Errors is null ? null : response.Errors.Select(item => item?.ToString()).Where(item => !string.IsNullOrWhiteSpace(item)).Select(item => item!).ToArray(),
            Data = response.Envelope?.Data,
            Items = response.Envelope?.Items,
            Envelope = new AslBelgiEnvelopeDto
            {
                Status = response.Status,
                Message = response.Message,
                Errors = response.Errors is null ? null : response.Errors.Select(item => item?.ToString()).Where(item => !string.IsNullOrWhiteSpace(item)).Select(item => item!).ToArray(),
                Data = response.Envelope?.Data,
                Items = response.Envelope?.Items
            },
            ApiKey = response.ApiKey,
            Id = response.Id,
            ExpiresOn = response.ExpiresOn,
            Label = response.Label
        };
    }
}
