using Integration.Didox.Configs;
using Integration.Shared.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace Integration.Didox.Http;

/// <summary>
/// Didox ikki qatlamli autentifikatsiyaga ega (INT_DIDOX.md §2.1) — ikkalasi ham HAR
/// SO'ROVDA yuboriladi:
///   - Partner-Authorization — hamkor tokeni, platforma darajasida (IOptions&lt;DidoxOptions&gt;).
///   - user-key — tashkilot tokeni, DidoxTokenCache'dan (organization_id bo'yicha).
/// Bu handler avtomatik login QILMAYDI — token yo'q/eskirgan bo'lsa, chaqiruvchi avval
/// DidoxAuthController orqali (GET /auth/challenge → frontend/e-imzo → POST /auth/complete)
/// yangi token olishi kerak (EdocsAuthorizationHandler bilan bir xil naqsh).
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

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var options = _options.Value;
        if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var baseUri)
            || !string.Equals(baseUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Didox BaseUrl must use HTTPS.");
        }

        if (string.IsNullOrWhiteSpace(options.PartnerToken))
        {
            throw new InvalidOperationException(
                "Didox:PartnerToken is not configured — set it in appsettings.json (obtained manually from the Didox account manager, INT_DIDOX.md §1.3).");
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

        request.Headers.TryAddWithoutValidation("Partner-Authorization", options.PartnerToken);
        request.Headers.TryAddWithoutValidation("user-key", token);

        _logger.LogDebug(
            "Sending Didox request {Method} to host {Host} for organization {OrganizationId}.",
            request.Method.Method,
            request.RequestUri?.Host ?? baseUri.Host,
            organizationId);

        return await base.SendAsync(request, cancellationToken);
    }
}
