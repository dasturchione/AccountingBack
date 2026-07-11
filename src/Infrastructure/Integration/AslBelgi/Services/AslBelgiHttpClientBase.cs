using Integration.AslBelgi.Configs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;

namespace Integration.AslBelgi.Services;

/// <summary>
/// Shared HTTP resilience primitives for the Asl Belgisi clients (client + auth client): named
/// client + timeout, base-url resolution, correlation-id propagation, and the transient-status
/// retry loop with exponential backoff. Behaviour is identical to the previous per-client copies.
///
/// Kept AslBelgi-local (bound to AslBelgiSettings) rather than reusing the Tax TaxProviderBase,
/// which is coupled to TaxIntegrationSettings / the ITaxProvider hierarchy.
/// </summary>
public abstract class AslBelgiHttpClientBase
{
    protected const string CorrelationHeaderName = "X-Correlation-Id";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    protected readonly AslBelgiSettings Settings;
    protected readonly ILogger Logger;

    protected AslBelgiHttpClientBase(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor,
        IOptions<AslBelgiSettings> settings,
        ILogger logger)
    {
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
        Settings = settings.Value;
        Logger = logger;
    }

    protected HttpClient CreateClient(string clientName)
    {
        var client = _httpClientFactory.CreateClient(clientName);
        client.Timeout = TimeSpan.FromSeconds(Math.Max(5, Settings.TimeoutSeconds));
        return client;
    }

    protected Uri BuildUri(string path)
    {
        var baseUrl = Settings.ServerBaseUrl.TrimEnd('/');
        var route = string.IsNullOrWhiteSpace(path) ? string.Empty : path.TrimStart('/');
        return new Uri(new Uri(baseUrl + "/"), route);
    }

    protected void ApplyCorrelationId(HttpRequestMessage request)
    {
        if (!Settings.EnableCorrelationPropagation)
            return;

        var correlationId = _httpContextAccessor.HttpContext?.TraceIdentifier;
        if (!string.IsNullOrWhiteSpace(correlationId))
            request.Headers.TryAddWithoutValidation(CorrelationHeaderName, correlationId);
    }

    protected async Task<HttpResponseMessage> SendWithRetryAsync(
        Func<Task<HttpResponseMessage>> send,
        int initialDelayMs,
        string context,
        CancellationToken ct)
    {
        var attempts = Math.Max(1, Settings.RetryCount);
        var delay = TimeSpan.FromMilliseconds(initialDelayMs);
        Exception? last = null;

        for (var attempt = 1; attempt <= attempts; attempt++)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                var response = await send();
                if (IsTransient(response.StatusCode) && attempt < attempts)
                {
                    Logger.LogWarning("AslBelgi {Context} returned transient status {StatusCode} on attempt {Attempt}/{Attempts}", context, (int)response.StatusCode, attempt, attempts);
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
                last = ex;
                if (attempt >= attempts)
                    break;

                Logger.LogWarning(ex, "AslBelgi {Context} attempt {Attempt}/{Attempts} failed", context, attempt, attempts);
                await Task.Delay(delay, ct);
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2);
            }
        }

        throw last ?? new HttpRequestException($"AslBelgi {context} failed after retries.");
    }

    protected static bool IsTransient(HttpStatusCode statusCode)
        => statusCode is HttpStatusCode.RequestTimeout
            or HttpStatusCode.TooManyRequests
            or HttpStatusCode.BadGateway
            or HttpStatusCode.ServiceUnavailable
            or HttpStatusCode.GatewayTimeout;

    protected static async Task<string> ReadBodySafelyAsync(HttpResponseMessage response, CancellationToken ct)
    {
        try
        {
            return await response.Content.ReadAsStringAsync(ct);
        }
        catch
        {
            return string.Empty;
        }
    }
}
