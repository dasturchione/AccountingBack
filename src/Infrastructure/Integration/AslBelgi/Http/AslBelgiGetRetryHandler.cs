using Microsoft.Extensions.Logging;

namespace Integration.AslBelgi.Http;

public sealed class AslBelgiGetRetryHandler : DelegatingHandler
{
    private const int MaxAttempts = 3;
    private static readonly TimeSpan InitialRetryDelay = TimeSpan.FromMilliseconds(200);

    private readonly ILogger<AslBelgiGetRetryHandler> _logger;

    public AslBelgiGetRetryHandler(ILogger<AslBelgiGetRetryHandler> logger)
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
                    "CRPT GET request returned transient status {StatusCode} on attempt {Attempt}/{Attempts}.",
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
                    "CRPT GET request failed with a transient network error on attempt {Attempt}/{Attempts}.",
                    attempt,
                    MaxAttempts);
            }

            await Task.Delay(GetRetryDelay(attempt), cancellationToken);
            retryRequest = CloneGetRequest(request);
        }

        throw new HttpRequestException("CRPT GET request failed after retry attempts.");
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

        foreach (var header in request.Headers)
        {
            if (string.Equals(header.Key, "Authorization", StringComparison.OrdinalIgnoreCase))
                continue;

            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        foreach (var option in request.Options)
            clone.Options.Set(new HttpRequestOptionsKey<object?>(option.Key), option.Value);

        return clone;
    }
}
