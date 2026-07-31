using Application.Abstractions.Integration;
using Integration.Edocs.Configs;
using Integration.Shared.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel.Constants;
using SharedKernel.Exceptions;
using System.Net.Http.Headers;

namespace Integration.Edocs.Http;

/// <summary>
/// E-DOCS avtorizatsiyasi: Bearer token FAQAT EdocsTokenCache'dan (tashkilot bo'yicha)
/// o'qiladi. Bu handler avtomatik qayta login QILMAYDI — token yo'q yoki muddati o'tgan
/// bo'lsa, chaqiruvchi avval EdocsAuthController orqali (GET /auth/challenge →
/// frontend/e-imzo bilan imzolash → POST /auth/complete) yangi token olishi kerak.
///
/// 6.5.7-bosqichdan boshlab: tashkilotning `edocs` integration_credential yozuvi (faol
/// bo'lishi shart) HAR bir so'rovda tekshiriladi; `/documents/*` so'rovlariga
/// x-product/x-partner sarlavhalari shu yerda, markazlashgan tarzda qo'shiladi
/// (avval EdocsFacturaService/EdocsDebugService alohida-alohida qo'shar edi).
/// </summary>
public sealed class EdocsAuthorizationHandler : DelegatingHandler
{
    private readonly IOptions<EdocsOptions> _options;
    private readonly EdocsTokenCache _tokenCache;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EdocsAuthorizationHandler> _logger;

    public EdocsAuthorizationHandler(
        IOptions<EdocsOptions> options,
        EdocsTokenCache tokenCache,
        IServiceScopeFactory scopeFactory,
        ILogger<EdocsAuthorizationHandler> logger)
    {
        _options = options;
        _tokenCache = tokenCache;
        _scopeFactory = scopeFactory;
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

        if (!request.Options.TryGetValue(IntegrationHttpRequestOptions.OrganizationId, out var organizationId))
        {
            throw new InvalidOperationException(
                "Edocs so'rovida organizationId topilmadi — chaqiruvchi request.Options ga IntegrationHttpRequestOptions.OrganizationId qo'yishi shart.");
        }

        // IIntegrationCredentialProvider Scoped (ichida AppDbContext bor), handler esa
        // ~2 daqiqa yashaydigan Transient — captive dependencydan qochish uchun har
        // chaqiruvda alohida DI scope yaratiladi.
        using var scope = _scopeFactory.CreateScope();
        var credentialProvider = scope.ServiceProvider.GetRequiredService<IIntegrationCredentialProvider>();
        var credential = await credentialProvider.GetAsync(organizationId, IntegrationProviderConst.Edocs, cancellationToken);

        if (credential is null)
        {
            throw new IntegrationUnauthorizedException(
                $"Edocs uchun tashkilot {organizationId} kredensiali topilmadi yoki faol emas (provider='{IntegrationProviderConst.Edocs}').");
        }

        if (!_tokenCache.TryGet(organizationId, out var token))
        {
            throw new EdocsAuthenticationRequiredException(
                "Edocs uchun qayta autentifikatsiya kerak — /auth/challenge va /auth/complete orqali.");
        }

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // 6.3-bosqichda aniqlangan, E-DOCS.pdf da qayd etilmagan sarlavhalar — faqat
        // /documents/ ostidagi so'rovlarga qo'shiladi (frontend bundle'dagi interceptor
        // bilan bir xil shart).
        if (IsDocumentsRequest(request))
        {
            if (!string.IsNullOrWhiteSpace(options.Product))
                request.Headers.TryAddWithoutValidation("x-product", options.Product);
            if (!string.IsNullOrWhiteSpace(credential.PartnerId))
                request.Headers.TryAddWithoutValidation("x-partner", credential.PartnerId);
        }

        _logger.LogDebug(
            "Sending Edocs request {Method} to host {Host} for organization {OrganizationId}.",
            request.Method.Method,
            request.RequestUri?.Host ?? baseUri.Host,
            organizationId);

        return await base.SendAsync(request, cancellationToken);
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
