using Microsoft.Extensions.Logging;

namespace Integration.Edocs.Http;

public sealed class EdocsGetRetryHandler : DelegatingHandler
{
    private const int MaxAttempts = 3;
    private static readonly TimeSpan InitialRetryDelay = TimeSpan.FromMilliseconds(200);

    private readonly ILogger<EdocsGetRetryHandler> _logger;

    public EdocsGetRetryHandler(ILogger<EdocsGetRetryHandler> logger)
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
                    "Edocs GET request returned transient status {StatusCode} on attempt {Attempt}/{Attempts}.",
                    (int)response.StatusCode,
                    attempt,
                    MaxAttempts);

                response.Dispose();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (HttpRequestException) when (attempt < MaxAttempts)
            {
                _logger.LogWarning(
                    "Edocs GET request failed with a transient network error on attempt {Attempt}/{Attempts}.",
                    attempt,
                    MaxAttempts);
            }

            await Task.Delay(GetRetryDelay(attempt), cancellationToken);
            retryRequest = CloneGetRequest(request);
        }

        throw new HttpRequestException("Edocs GET request failed after retry attempts.");
    }

    private static bool IsTransient(HttpResponseMessage response)
        => (int)response.StatusCode is >= 500 and <= 599;

    private static TimeSpan GetRetryDelay(int attempt)
        => TimeSpan.FromMilliseconds(Math.Min(InitialRetryDelay.TotalMilliseconds * Math.Pow(2, attempt - 1), 2000));

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
