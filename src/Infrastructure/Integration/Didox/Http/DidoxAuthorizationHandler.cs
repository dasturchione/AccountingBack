using Integration.Didox.Configs;
using Integration.Shared.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Integration.Didox.Http;

/// <summary>
/// Didox requestlari user-key bilan yuboriladi. Partner-Authorization
/// konfiguratsiyada mavjud bo'lsa qo'shimcha header sifatida yuboriladi.
/// </summary>
public sealed class DidoxAuthorizationHandler : DelegatingHandler
{
    private readonly IOptions<DidoxOptions> _options;
    private readonly DidoxTokenCache _tokenCache;
    private readonly ILogger<DidoxAuthorizationHandler> _logger;

    public DidoxAuthorizationHandler(
        IOptions<DidoxOptions> options,
        DidoxTokenCache tokenCache,
        ILogger<DidoxAuthorizationHandler> logger)
    {
        _options = options;
        _tokenCache = tokenCache;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var options = _options.Value;
        if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var baseUri)
            || !string.Equals(baseUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Didox BaseUrl must use HTTPS.");
        }

        if (!request.Options.TryGetValue(IntegrationHttpRequestOptions.OrganizationId, out var organizationId))
        {
            throw new InvalidOperationException(
                "Didox so'rovida organizationId topilmadi — chaqiruvchi request.Options ga IntegrationHttpRequestOptions.OrganizationId qo'yishi shart.");
        }

        if (!_tokenCache.TryGet(organizationId, out var token))
        {
            throw new DidoxAuthenticationRequiredException(
                "Didox uchun qayta autentifikatsiya kerak — /auth/challenge va /auth/complete orqali.");
        }

        if (!options.UsePartnerlessLegacyApi && !string.IsNullOrWhiteSpace(options.PartnerToken))
        {
            request.Headers.TryAddWithoutValidation("Partner-Authorization", options.PartnerToken);
        }

        request.Headers.TryAddWithoutValidation("user-key", token);

        _logger.LogDebug(
            "Sending Didox request {Method} to host {Host} for organization {OrganizationId}.",
            request.Method.Method,
            request.RequestUri?.Host ?? baseUri.Host,
            organizationId);

        return await base.SendAsync(request, cancellationToken);
    }
}
