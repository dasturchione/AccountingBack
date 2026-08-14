using Microsoft.Extensions.Logging;
using System.Net;

namespace Integration.Didox.Http;

public sealed class DidoxGetRetryHandler : DelegatingHandler
{
    private const int MaxAttempts = 3;
    private static readonly TimeSpan InitialRetryDelay = TimeSpan.FromMilliseconds(200);

    private readonly ILogger<DidoxGetRetryHandler> _logger;

    public DidoxGetRetryHandler(ILogger<DidoxGetRetryHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Method != HttpMethod.Get)
            return await base.SendAsync(request, cancellationToken);

        var retryRequest = request;
        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var response = await base.SendAsync(retryRequest, cancellationToken);
                if (!IsTransient(response) || attempt == MaxAttempts)
                    return response;

                _logger.LogWarning(
                    "Didox GET request returned transient status {StatusCode} on attempt {Attempt}/{Attempts}.",
                    (int)response.StatusCode,
                    attempt,
                    MaxAttempts);

                var retryDelay = GetRetryDelay(response, attempt);
                response.Dispose();
                await Task.Delay(retryDelay, cancellationToken);
                retryRequest = CloneGetRequest(request);
                continue;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested
                                                     && attempt < MaxAttempts)
            {
                _logger.LogWarning(
                    "Didox GET request timed out on attempt {Attempt}/{Attempts}.",
                    attempt,
                    MaxAttempts);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (HttpRequestException) when (attempt < MaxAttempts)
            {
                _logger.LogWarning(
                    "Didox GET request failed with a transient network error on attempt {Attempt}/{Attempts}.",
                    attempt,
                    MaxAttempts);
            }

            await Task.Delay(GetRetryDelay(attempt), cancellationToken);
            retryRequest = CloneGetRequest(request);
        }

        throw new HttpRequestException("Didox GET request failed after retry attempts.");
    }

    private static bool IsTransient(HttpResponseMessage response)
        => response.StatusCode == HttpStatusCode.TooManyRequests
           || (int)response.StatusCode is >= 500 and <= 599;

    private static TimeSpan GetRetryDelay(int attempt)
        => TimeSpan.FromMilliseconds(
            Math.Min(InitialRetryDelay.TotalMilliseconds * Math.Pow(2, attempt - 1), 2000)
            + Random.Shared.Next(25, 126));

    private static TimeSpan GetRetryDelay(HttpResponseMessage response, int attempt)
    {
        var retryAfter = response.Headers.RetryAfter;
        var delay = retryAfter?.Delta
            ?? (retryAfter?.Date - DateTimeOffset.UtcNow);
        return delay is { } value && value > TimeSpan.Zero
            ? TimeSpan.FromSeconds(Math.Min(value.TotalSeconds, 30))
            : GetRetryDelay(attempt);
    }

    private static HttpRequestMessage CloneGetRequest(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(HttpMethod.Get, request.RequestUri)
        {
            Version = request.Version,
            VersionPolicy = request.VersionPolicy
        };

        // Authorization ham ko'chiriladi: auth handler pipeline'da bu handler'dan
        // TASHQARIDA turadi, ya'ni qayta urinishda u boshqa ishlamaydi. Sarlavha
        // tashlab ketilsa, 2-urinishdan boshlab so'rov autentifikatsiyasiz ketardi.
        foreach (var header in request.Headers)
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

        foreach (var option in request.Options)
            clone.Options.Set(new HttpRequestOptionsKey<object?>(option.Key), option.Value);

        return clone;
    }
}
