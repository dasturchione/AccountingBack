using Integration.Faktura.Configs;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;

namespace Integration.Faktura.Http;

// 6.5.7-bosqichda tasdiqlandi: Faktura kredensiali (Username/Password/ClientId/ClientSecret)
// tashkilotga tegishli EMAS — bu integratsiyaning yagona chaqiruvchisi
// (OrganizationService.GetByInnAsync, [AllowAnonymous] "by-inn" endpoint) tashkilot hali
// yaratilmasdan turib ishlaydi. Shuning uchun bu handler organizationId/
// IIntegrationCredentialProvider dan FOYDALANMAYDI — kredensial platforma darajasida,
// IOptions<FakturaOptions> orqali, bitta umumiy token keshi bilan qoladi.
public sealed class FakturaAuthorizationHandler : DelegatingHandler
{
    private readonly IFakturaTokenService _tokenService;
    private readonly ILogger<FakturaAuthorizationHandler> _logger;

    public FakturaAuthorizationHandler(
        IFakturaTokenService tokenService,
        ILogger<FakturaAuthorizationHandler> logger)
    {
        _tokenService = tokenService;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _tokenService.GetAccessTokenAsync(cancellationToken);

        // Token faqat shu so'rovga qo'yiladi; DefaultRequestHeaders'ga tegilmaydi.
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var authorization = request.Headers.Authorization;

        _logger.LogDebug(
            "Sending Faktura request {Method} to host {Host}, path {Path}; authorizationPresent={AuthorizationPresent}, authorizationScheme={AuthorizationScheme}.",
            request.Method.Method,
            request.RequestUri?.Host,
            request.RequestUri?.AbsolutePath,
            authorization is not null,
            authorization?.Scheme ?? "none");

        return await base.SendAsync(request, cancellationToken);
    }

}
