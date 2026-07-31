using Integration.Edocs.Configs;
using Integration.Shared.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace Integration.Edocs.Http;

/// <summary>
/// E-DOCS avtorizatsiyasi: token FAQAT EdocsTokenCache'dan o'qiladi. Bu handler
/// avtomatik qayta login QILMAYDI — token yo'q yoki muddati o'tgan bo'lsa, chaqiruvchi
/// avval EdocsAuthController orqali (GET /auth/challenge → frontend/e-imzo bilan
/// imzolash → POST /auth/complete) yangi token olishi kerak.
///
/// 6.1-bosqichdagi login/parol (POST /loginpassword) yo'li 6.2-bosqichda OLIB TASHLANDI:
/// endi faqat ЭЦП orqali (POST /login, authId + pkcs7) kirish qo'llab-quvvatlanadi.
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

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var options = _options.Value;
        if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var baseUri)
            || !string.Equals(baseUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Edocs BaseUrl must use HTTPS.");
        }

        // 6.5.6-bosqich (poydevor): chaqiruvchi hali request.Options ga organizationId
        // qo'ymaydi (bu — keyingi bosqichda, servis darajasidagi Option A ulanishi bilan
        // birga qilinadi). Shu bosqichgacha 0 — "hali tashkilot bo'yicha ajratilmagan"
        // bucket'ini bildiradi; kesh imzosi tayyor, lekin haqiqiy ko'p-tashkilotli
        // ishlash keyingi bosqichda boshlanadi.
        var organizationId = request.Options.TryGetValue(IntegrationHttpRequestOptions.OrganizationId, out var orgId) ? orgId : 0;

        if (!_tokenCache.TryGet(organizationId, out var token))
        {
            throw new EdocsAuthenticationRequiredException(
                "Edocs uchun qayta autentifikatsiya kerak — /auth/challenge va /auth/complete orqali.");
        }

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        _logger.LogDebug(
            "Sending Edocs request {Method} to host {Host}.",
            request.Method.Method,
            request.RequestUri?.Host ?? baseUri.Host);

        return await base.SendAsync(request, cancellationToken);
    }
}
