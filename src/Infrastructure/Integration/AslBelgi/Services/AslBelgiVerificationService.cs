using Application.Features.Integration.AslBelgi.DTOs;
using Application.Features.Integration.AslBelgi.Services;
using Integration.AslBelgi.Http;
using Integration.AslBelgi.Configs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SharedKernel.Exceptions;
using SharedKernel.Security;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Integration.AslBelgi.Services;

public sealed class AslBelgiVerificationService : IAslBelgiVerificationService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHostEnvironment? _environment;
    private readonly string? _apiKey;

    public AslBelgiVerificationService(IHttpClientFactory httpClientFactory, IHostEnvironment? environment = null, IOptions<AslBelgiOptions>? options = null)
    {
        _httpClientFactory = httpClientFactory;
        _environment = environment;
        _apiKey = options?.Value.ApiKey;
    }

    public Task<JsonElement> GetPublicCodeInformationAsync(MarkingCodeCheckRequestDto request, CancellationToken ct = default)
        => SendJsonAsync(HttpMethod.Post, "public/api/cod/public/codes", request, ct);

    public Task<JsonElement> GetPrivateCodeInformationAsync(MarkingCodeCheckRequestDto request, CancellationToken ct = default)
        => SendJsonAsync(HttpMethod.Post, "public/api/cod/private/codes", request, ct);

    public Task<JsonElement> GetProductsByGtinAsync(ProductRegistryByGtinRequestDto request, CancellationToken ct = default)
    {
        var path = $"public/api/v1/product-registry/product?productGroup={Uri.EscapeDataString(request.ProductGroup)}&gtin={Uri.EscapeDataString(request.Gtin)}";
        return SendJsonAsync(HttpMethod.Get, path, body: null, ct);
    }

    public async Task<CounterpartyStatusResponseDto?> GetCounterpartyStatusAsync(string tin, CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"public/api/v1/party/parties/{Uri.EscapeDataString(tin)}/status");
        using var response = await SendAsync(request, ct);

        if (response.StatusCode == HttpStatusCode.NoContent)
            return null;

        await EnsureSuccessStatusOrThrowAsync(response);
        var content = await response.Content.ReadAsStringAsync(ct);

        if (string.IsNullOrWhiteSpace(content))
            return null;

        return JsonSerializer.Deserialize<CounterpartyStatusResponseDto>(content, JsonOptions);
    }

    private async Task<JsonElement> SendJsonAsync(HttpMethod method, string path, object? body, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(method, path);
        if (body is not null)
            request.Content = JsonContent.Create(body, options: JsonOptions);

        using var response = await SendAsync(request, ct);
        await EnsureSuccessStatusOrThrowAsync(response);
        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
        return document.RootElement.Clone();
    }

    private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        try
        {
            var client = _httpClientFactory.CreateClient(AslBelgiHttpClientNames.Client);
            return await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new IntegrationHttpException("CRPT request timed out.", StatusCodes.Status504GatewayTimeout);
        }
        catch (HttpRequestException)
        {
            throw new IntegrationHttpException("CRPT request could not be completed.", StatusCodes.Status502BadGateway);
        }
    }

    private async Task EnsureSuccessStatusOrThrowAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        if (response.StatusCode == HttpStatusCode.BadRequest && _environment?.IsDevelopment() == true)
        {
            var detail = SensitiveDataRedactor.Redact(await response.Content.ReadAsStringAsync(), _apiKey);
            throw new IntegrationHttpException($"CRPT request failed with HTTP status 400. Detail: {detail}", 400);
        }

        throw response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => new IntegrationUnauthorizedException("CRPT credentials were rejected."),
            HttpStatusCode.Forbidden => new IntegrationForbiddenException("CRPT denied the request."),
            _ => new IntegrationHttpException(
                $"CRPT request failed with HTTP status {(int)response.StatusCode}.",
                (int)response.StatusCode)
        };
    }

}
