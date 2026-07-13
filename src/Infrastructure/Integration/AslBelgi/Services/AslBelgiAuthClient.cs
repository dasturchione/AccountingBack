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
        return await PostContentAsync<AslBelgiAuthResponse>(
            Settings.RefreshPath,
            () => new FormUrlEncodedContent([
                new KeyValuePair<string, string>("refreshToken", request.RefreshToken)
            ]),
            ct);
    }

    private Task<TResponse> PostAsync<TRequest, TResponse>(string path, TRequest body, CancellationToken ct)
    {
        return PostContentAsync<TResponse>(
            path,
            () => new StringContent(JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json"),
            ct);
    }

    private async Task<TResponse> PostContentAsync<TResponse>(string path, Func<HttpContent> createContent, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(Settings.ServerBaseUrl))
            throw new InvalidOperationException("AslBelgi server URL is not configured.");

        var client = CreateClient(ClientName);
        async Task<HttpResponseMessage> SendRequestAsync()
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, BuildUri(path)) { Content = createContent() };
            ApplyCorrelationId(request);
            return await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        }

        using var response = await SendWithRetryAsync(
            SendRequestAsync,
            InitialRetryDelayMs,
            "auth request",
            ct,
            retrySafe: false);

        if (response.IsSuccessStatusCode)
        {
            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            var token = await JsonSerializer.DeserializeAsync<TResponse>(stream, JsonOptions, ct);
            return token ?? throw new InvalidOperationException("AslBelgi auth response body is empty.");
        }

        throw CreateHttpException(response.StatusCode);
    }

    private static Exception CreateHttpException(HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.Unauthorized => new IntegrationUnauthorizedException("AslBelgi authentication rejected."),
            HttpStatusCode.Forbidden => new IntegrationForbiddenException("AslBelgi access forbidden."),
            _ => new IntegrationHttpException($"AslBelgi request failed with HTTP status {(int)statusCode}.", (int)statusCode)
        };
    }
}
