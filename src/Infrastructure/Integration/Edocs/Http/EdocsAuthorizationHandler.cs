using Integration.Edocs.Configs;
using Integration.Shared.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel.Constants;
using SharedKernel.Exceptions;
using System.Net.Http.Headers;

namespace Integration.Edocs.Http;

/// <summary>
/// E-DOCS avtorizatsiyasi: Bearer token FAQAT EdocsTokenCache'dan o'qiladi.
/// Platforma konfiguratsiyasi Product va PartnerId qiymatlarini appsettings.json
/// dagi EdocsOptions orqali beradi.
/// </summary>
public sealed class EdocsAuthorizationHandler : DelegatingHandler
{
    private readonly IOptions<EdocsOptions> _options;
    private readonly EdocsTokenCache _tokenCache;
    private readonly ILogger<EdocsAuthorizationHandler> _logger;

    public EdocsAuthorizationHandler(
        IOptions<EdocsOptions> options,
        EdocsTokenCache tokenCache,
        ILogger<EdocsAuthorizationHandler> logger)
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
            throw new InvalidOperationException("Edocs BaseUrl must use HTTPS.");
        }

        if (!request.Options.TryGetValue(
                IntegrationHttpRequestOptions.OrganizationId,
                out var organizationId))
        {
            throw new InvalidOperationException(
                "Edocs so'rovida organizationId topilmadi — chaqiruvchi request.Options ga IntegrationHttpRequestOptions.OrganizationId qo'yishi shart.");
        }

        if (!_tokenCache.TryGet(organizationId, IntegrationProviderConst.Edocs, out var token))
        {
            throw new EdocsAuthenticationRequiredException(
                "Edocs uchun qayta autentifikatsiya kerak — /auth/challenge va /auth/complete orqali.");
        }

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        if (IsDocumentsRequest(request))
        {
            if (string.IsNullOrWhiteSpace(options.Product)
                || string.IsNullOrWhiteSpace(options.PartnerId))
            {
                throw new InvalidOperationException(
                    "Edocs Product va PartnerId appsettings.json dagi Edocs bo'limida sozlanishi kerak.");
            }

            request.Headers.TryAddWithoutValidation("x-product", options.Product);
            request.Headers.TryAddWithoutValidation("x-partner", options.PartnerId);
        }

        _logger.LogDebug(
            "Sending Edocs request {Method} to host {Host} for organization {OrganizationId}.",
            request.Method.Method,
            request.RequestUri?.Host ?? baseUri.Host,
            organizationId);

        var response = await base.SendAsync(request, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            response.Dispose();
            _tokenCache.Clear(organizationId, IntegrationProviderConst.Edocs);
            throw new EdocsAuthenticationRequiredException(
                "Edocs provider 401 qaytardi; nazoratli qayta autentifikatsiya kerak.");
        }

        return response;
    }

    private static bool IsDocumentsRequest(HttpRequestMessage request)
    {
        if (request.RequestUri is null)
            return false;

        var path = request.RequestUri.IsAbsoluteUri
            ? request.RequestUri.AbsolutePath
            : request.RequestUri.OriginalString;

        return path.TrimStart('/').StartsWith("documents/", StringComparison.OrdinalIgnoreCase);
    }
}
