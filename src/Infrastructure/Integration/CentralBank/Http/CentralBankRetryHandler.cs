using Integration.CentralBank.Configs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;

namespace Integration.CentralBank.Http;

/// <summary>
/// Markaziy bank API'si ochiq — kalit yoki token talab qilmaydi, shuning uchun
/// bu integratsiyada authorization handler ataylab yaratilmagan.
/// Handler faqat GET so'rovlarini qayta uradi; provayderdagi ikkala chaqiruv
/// (kunlik va sana bo'yicha kurslar) o'qish amali va GET orqali bajariladi.
/// </summary>
public sealed class CentralBankRetryHandler : DelegatingHandler
{
    private static readonly TimeSpan InitialRetryDelay = TimeSpan.FromMilliseconds(300);

    private readonly IOptions<CentralBankOptions> _options;
    private readonly ILogger<CentralBankRetryHandler> _logger;

    public CentralBankRetryHandler(IOptions<CentralBankOptions> options, ILogger<CentralBankRetryHandler> logger)
    {
        _options = options;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Method != HttpMethod.Get)
            return await base.SendAsync(request, cancellationToken);

        var maxAttempts = Math.Max(1, _options.Value.RetryCount);
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
                    "Central Bank GET request returned transient status {StatusCode} on attempt {Attempt}/{Attempts}.",
                    (int)response.StatusCode,
                    attempt,
                    maxAttempts);

                response.Dispose();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (HttpRequestException) when (attempt < maxAttempts)
            {
                _logger.LogWarning(
                    "Central Bank GET request failed with a transient network error on attempt {Attempt}/{Attempts}.",
                    attempt,
                    maxAttempts);
            }

            await Task.Delay(delay, cancellationToken);
            delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2);
            retryRequest = CloneGetRequest(request);
        }

        throw new HttpRequestException("Central Bank GET request failed after retry attempts.");
    }

    private static bool IsTransient(HttpStatusCode statusCode)
        => (int)statusCode is >= 500 and <= 599
            || statusCode is HttpStatusCode.RequestTimeout or HttpStatusCode.TooManyRequests;

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
