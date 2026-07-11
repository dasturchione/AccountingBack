using Application.Abstractions.Integration;
using Integration.Tax.Configs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel.Exceptions;
using System.Text.Json;
using System.Text;

namespace Integration.Tax.Providers;

public abstract class TaxProviderBase : ITaxProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private const string CorrelationHeaderName = "X-Correlation-Id";
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    protected readonly TaxIntegrationSettings Settings;
    protected readonly ILogger Logger;

    protected TaxProviderBase(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, IOptions<TaxIntegrationSettings> options, ILogger logger)
    {
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
        Settings = options.Value;
        Logger = logger;
    }

    public abstract string Code { get; }
    public abstract string Name { get; }

    public virtual Task<TaxProviderStatusDto> GetStatusAsync(CancellationToken ct = default)
    {
        var providerSettings = ResolveProviderSettings();
        return Task.FromResult(new TaxProviderStatusDto
        {
            ProviderCode = Code,
            ProviderName = Name,
            IsEnabled = Settings.Enabled && providerSettings.Enabled,
            IsConfigured = !string.IsNullOrWhiteSpace(providerSettings.BaseUrl),
            Endpoint = providerSettings.BaseUrl,
            CheckedAt = DateTime.UtcNow
        });
    }

    protected abstract TaxIntegrationSettings.ProviderSettings ResolveProviderSettings();

    protected HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient(Settings.ClientName ?? "TaxIntegration");
        var providerSettings = ResolveProviderSettings();
        var timeoutSeconds = providerSettings.TimeoutSeconds ?? Settings.TimeoutSeconds;
        client.Timeout = TimeSpan.FromSeconds(Math.Max(5, timeoutSeconds));
        return client;
    }

    protected Uri BuildUri(string path, IReadOnlyDictionary<string, string?>? query = null)
    {
        var providerSettings = ResolveProviderSettings();
        if (string.IsNullOrWhiteSpace(providerSettings.BaseUrl))
            throw new InvalidOperationException($"Tax provider {Code} is not configured.");

        var builder = new UriBuilder(new Uri(new Uri(providerSettings.BaseUrl.TrimEnd('/') + "/"), path.TrimStart('/')));

        if (query is { Count: > 0 })
        {
            var queryString = string.Join("&", query
                .Where(x => !string.IsNullOrWhiteSpace(x.Value))
                .Select(x => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value!)}"));
            builder.Query = queryString;
        }

        return builder.Uri;
    }

    protected async Task<T?> GetAsync<T>(string path, IReadOnlyDictionary<string, string?>? query, CancellationToken ct)
    {
        var client = CreateClient();
        using var response = await SendWithRetryAsync(() =>
        {
            var request = new HttpRequestMessage(HttpMethod.Get, BuildUri(path, query));
            ApplyCorrelationId(request);
            return client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        }, ct);
        await EnsureSuccessStatusOrThrowAsync(response);
        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        return await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions, ct);
    }

    protected async Task<TResponse?> PostAsync<TRequest, TResponse>(string path, TRequest body, CancellationToken ct)
    {
        var client = CreateClient();
        using var response = await SendWithRetryAsync(() =>
        {
            var request = new HttpRequestMessage(HttpMethod.Post, BuildUri(path))
            {
                Content = new StringContent(JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json")
            };
            ApplyCorrelationId(request);
            return client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        }, ct);
        await EnsureSuccessStatusOrThrowAsync(response);
        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        return await JsonSerializer.DeserializeAsync<TResponse>(stream, JsonOptions, ct);
    }

    protected async Task<HttpResponseMessage> SendWithRetryAsync(Func<Task<HttpResponseMessage>> action, CancellationToken ct)
    {
        var attempts = Math.Max(1, ResolveRetryCount());
        var delay = TimeSpan.FromMilliseconds(200);
        Exception? lastException = null;

        for (var attempt = 1; attempt <= attempts; attempt++)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                var response = await action();
                if (IsTransient(response.StatusCode) && attempt < attempts)
                {
                    Logger.LogWarning("Tax provider {Provider} returned transient status {StatusCode} on attempt {Attempt}/{Attempts}", Code, (int)response.StatusCode, attempt, attempts);
                    response.Dispose();
                    await Task.Delay(delay, ct);
                    delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2);
                    continue;
                }

                return response;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                lastException = ex;
                if (attempt >= attempts)
                    break;

                Logger.LogWarning(ex, "Tax provider {Provider} request attempt {Attempt}/{Attempts} failed", Code, attempt, attempts);
                await Task.Delay(delay, ct);
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2);
            }
        }

        throw lastException ?? new HttpRequestException($"Tax provider {Code} request failed.");
    }

    private int ResolveRetryCount()
    {
        var providerSettings = ResolveProviderSettings();
        return providerSettings.RetryCount ?? Settings.RetryCount;
    }

    protected async Task EnsureSuccessStatusOrThrowAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        var responseBody = await ReadResponseBodySafelyAsync(response);
        var detail = string.IsNullOrWhiteSpace(responseBody)
            ? response.ReasonPhrase ?? "Integration request failed."
            : responseBody;

        throw new IntegrationHttpException(detail, (int)response.StatusCode);
    }

    private static async Task<string> ReadResponseBodySafelyAsync(HttpResponseMessage response)
    {
        try
        {
            return await response.Content.ReadAsStringAsync();
        }
        catch
        {
            return string.Empty;
        }
    }

    private void ApplyCorrelationId(HttpRequestMessage request)
    {
        if (!Settings.EnableCorrelationPropagation)
            return;

        var correlationId = _httpContextAccessor.HttpContext?.TraceIdentifier;
        if (!string.IsNullOrWhiteSpace(correlationId))
            request.Headers.TryAddWithoutValidation(CorrelationHeaderName, correlationId);
    }

    private static bool IsTransient(System.Net.HttpStatusCode statusCode)
        => statusCode is System.Net.HttpStatusCode.RequestTimeout
            or System.Net.HttpStatusCode.TooManyRequests
            or System.Net.HttpStatusCode.BadGateway
            or System.Net.HttpStatusCode.ServiceUnavailable
            or System.Net.HttpStatusCode.GatewayTimeout;
}
