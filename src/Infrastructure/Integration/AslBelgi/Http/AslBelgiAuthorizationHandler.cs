using Integration.AslBelgi.Configs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace Integration.AslBelgi.Http;

public sealed class AslBelgiAuthorizationHandler : DelegatingHandler
{
    private readonly IOptions<AslBelgiOptions> _options;
    private readonly ILogger<AslBelgiAuthorizationHandler> _logger;

    public AslBelgiAuthorizationHandler(
        IOptions<AslBelgiOptions> options,
        ILogger<AslBelgiAuthorizationHandler> logger)
    {
        _options = options;
        _logger = logger;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var options = _options.Value;
        if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var baseUri)
            || !string.Equals(baseUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("CRPT BaseUrl must use HTTPS.");
        }

        if (request.RequestUri is { IsAbsoluteUri: true } requestUri
            && !string.Equals(requestUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("CRPT requests must use HTTPS.");
        }

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiKey);

        _logger.LogDebug(
            "Sending CRPT request {Method} to host {Host}.",
            request.Method.Method,
            request.RequestUri?.Host ?? baseUri.Host);

        return base.SendAsync(request, cancellationToken);
    }
}
