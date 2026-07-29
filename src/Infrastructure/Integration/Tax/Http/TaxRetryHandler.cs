using Integration.Tax.Configs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;

namespace Integration.Tax.Http;

/// <summary>
/// Faqat GET so'rovlarini qayta uradi. Soliq provayderlaridagi barcha o'qish
/// amallari (MXIK qidiruv/lookup, SoliqApi qidiruv/lookup) GET orqali bajariladi.
/// Yagona POST — EFaktura hujjat yuborish/bekor qilish, ya'ni yozish amali;
/// u qayta urilmaydi.
/// </summary>
public sealed class TaxRetryHandler : DelegatingHandler
{
    private const double MaxRetryDelayMs = 2000;
    private static readonly TimeSpan InitialRetryDelay = TimeSpan.FromMilliseconds(200);

    private readonly IOptions<TaxIntegrationOptions> _options;
    private readonly ILogger<TaxRetryHandler> _logger;

    public TaxRetryHandler(IOptions<TaxIntegrationOptions> options, ILogger<TaxRetryHandler> logger)
    {
        _options = options;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Method != HttpMethod.Get)
            return await base.SendAsync(request, cancellationToken);

        var maxAttempts = Math.Clamp(_options.Value.RetryCount, 1, 3);
        var delay = InitialRetryDelay;
        var retryRequest = request;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var response = await base.SendAsync(retryRequest, cancellationToken);
                if (!IsTransient(response.StatusCode) || attempt == maxAttempts)
                    return response;

                _logger.LogWarning(
                    "Tax GET request returned transient status {StatusCode} on attempt {Attempt}/{Attempts}.",
                    (int)response.StatusCode,
                    attempt,
                    maxAttempts);

                delay = GetRetryDelay(response, delay);
                response.Dispose();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (HttpRequestException) when (attempt < maxAttempts)
            {
                _logger.LogWarning(
                    "Tax GET request failed with a transient network error on attempt {Attempt}/{Attempts}.",
                    attempt,
                    maxAttempts);
            }

            await Task.Delay(delay, cancellationToken);
            delay = TimeSpan.FromMilliseconds(Math.Min(delay.TotalMilliseconds * 2, MaxRetryDelayMs));
            retryRequest = CloneGetRequest(request);
        }

        throw new HttpRequestException("Tax GET request failed after retry attempts.");
    }

    private static bool IsTransient(HttpStatusCode statusCode)
        => statusCode is HttpStatusCode.RequestTimeout
            or HttpStatusCode.TooManyRequests
            or HttpStatusCode.BadGateway
            or HttpStatusCode.ServiceUnavailable
            or HttpStatusCode.GatewayTimeout;

    private static TimeSpan GetRetryDelay(HttpResponseMessage response, TimeSpan fallback)
    {
        if (response.Headers.RetryAfter is { } retryAfter)
        {
            var retryAfterMs = retryAfter.Delta?.TotalMilliseconds
                ?? (retryAfter.Date is { } date
                    ? (date - DateTimeOffset.UtcNow).TotalMilliseconds
                    : fallback.TotalMilliseconds);

            if (retryAfterMs >= 0)
                return TimeSpan.FromMilliseconds(Math.Min(retryAfterMs, MaxRetryDelayMs));
        }

        return TimeSpan.FromMilliseconds(Math.Min(fallback.TotalMilliseconds, MaxRetryDelayMs));
    }

    private static HttpRequestMessage CloneGetRequest(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(HttpMethod.Get, request.RequestUri)
        {
            Version = request.Version,
            VersionPolicy = request.VersionPolicy
        };

        foreach (var header in request.Headers)
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

        foreach (var option in request.Options)
            clone.Options.Set(new HttpRequestOptionsKey<object?>(option.Key), option.Value);

        return clone;
    }
}
