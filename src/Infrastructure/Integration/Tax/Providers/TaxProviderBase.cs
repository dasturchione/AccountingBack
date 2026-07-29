using Application.Abstractions.Integration;
using Integration.Tax.Configs;
using Integration.Tax.Http;
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
    protected readonly TaxIntegrationOptions Settings;
    protected readonly ILogger Logger;

    protected TaxProviderBase(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, IOptions<TaxIntegrationOptions> options, ILogger logger)
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

    protected abstract TaxIntegrationOptions.ProviderOptions ResolveProviderSettings();

    protected HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient(Settings.ClientName ?? TaxHttpClientNames.Client);
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
        using var request = new HttpRequestMessage(HttpMethod.Get, BuildUri(path, query));
        ApplyCorrelationId(request);

        // Qayta urinish TaxRetryHandler'da bajariladi.
        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        await EnsureSuccessStatusOrThrowAsync(response);
        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        return await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions, ct);
    }

    protected async Task<TResponse?> PostAsync<TRequest, TResponse>(string path, TRequest body, CancellationToken ct)
    {
        var client = CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, BuildUri(path))
        {
            Content = new StringContent(JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json")
        };
        ApplyCorrelationId(request);

        // POST — yozish amali; handler uni ataylab qayta urmaydi.
        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        await EnsureSuccessStatusOrThrowAsync(response);
        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        return await JsonSerializer.DeserializeAsync<TResponse>(stream, JsonOptions, ct);
    }

    protected async Task EnsureSuccessStatusOrThrowAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        // External bodies can contain tokens, credentials or provider PII. Keep the exception
        // deliberately status-only; callers can map it to their safe provider error.
        throw new IntegrationHttpException(
            $"Integration request failed with HTTP status {(int)response.StatusCode}.",
            (int)response.StatusCode);
    }

    private void ApplyCorrelationId(HttpRequestMessage request)
    {
        if (!Settings.EnableCorrelationPropagation)
            return;

        var correlationId = _httpContextAccessor.HttpContext?.TraceIdentifier;
        if (!string.IsNullOrWhiteSpace(correlationId))
            request.Headers.TryAddWithoutValidation(CorrelationHeaderName, correlationId);
    }
}
