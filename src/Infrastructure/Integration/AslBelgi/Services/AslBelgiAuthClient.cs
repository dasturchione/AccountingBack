using Integration.AslBelgi.Abstractions;
using Integration.AslBelgi.Configs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel.Exceptions;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Integration.AslBelgi.Services;

public sealed class AslBelgiAuthClient : AslBelgiHttpClientBase, IAslBelgiAuthClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private const string ClientName = "AslBelgiAuth";
    private const int InitialRetryDelayMs = 200;

    public AslBelgiAuthClient(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor,
        IOptions<AslBelgiSettings> settings,
        ILogger<AslBelgiAuthClient> logger)
        : base(httpClientFactory, httpContextAccessor, settings, logger)
    {
    }

    public async Task<AslBelgiAuthResponse> AuthenticateAsync(AslBelgiAuthRequest request, CancellationToken ct = default)
    {
        return await PostAsync<AslBelgiAuthRequest, AslBelgiAuthResponse>(Settings.AuthenticatePath, request, ct);
    }

    public async Task<AslBelgiAuthResponse> RefreshAsync(AslBelgiRefreshRequest request, CancellationToken ct = default)
    {
        // Per the Asl Belgisi doc, refresh is application/x-www-form-urlencoded with refreshToken={...},
        // not JSON like authenticate.
        var form = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("refreshToken", request.RefreshToken)
        ]);

        return await PostContentAsync<AslBelgiAuthResponse>(Settings.RefreshPath, form, ct);
    }

    private Task<TResponse> PostAsync<TRequest, TResponse>(string path, TRequest body, CancellationToken ct)
    {
        var content = new StringContent(JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json");
        return PostContentAsync<TResponse>(path, content, ct);
    }

    private async Task<TResponse> PostContentAsync<TResponse>(string path, HttpContent content, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(Settings.ServerBaseUrl))
            throw new InvalidOperationException("AslBelgi server URL is not configured.");

        var client = CreateClient(ClientName);
        using var request = new HttpRequestMessage(HttpMethod.Post, BuildUri(path)) { Content = content };

        ApplyCorrelationId(request);

        using var response = await SendWithRetryAsync(
            () => client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct),
            InitialRetryDelayMs,
            "auth request",
            ct);

        if (response.IsSuccessStatusCode)
        {
            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            var token = await JsonSerializer.DeserializeAsync<TResponse>(stream, JsonOptions, ct);
            return token ?? throw new InvalidOperationException("AslBelgi auth response body is empty.");
        }

        var detail = await ReadBodySafelyAsync(response, ct);
        throw CreateHttpException(response.StatusCode, detail);
    }

    private static Exception CreateHttpException(HttpStatusCode statusCode, string detail)
    {
        return statusCode switch
        {
            HttpStatusCode.Unauthorized => new IntegrationUnauthorizedException("AslBelgi authentication rejected." + detail),
            HttpStatusCode.Forbidden => new IntegrationForbiddenException("AslBelgi access forbidden." + detail),
            _ => new IntegrationHttpException($"AslBelgi request failed with status {(int)statusCode}. {detail}", (int)statusCode)
        };
    }
}
