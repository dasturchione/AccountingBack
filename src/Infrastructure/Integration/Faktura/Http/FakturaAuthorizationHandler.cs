using Application.Abstractions.Authentication;
using Application.Abstractions.Integration.Edo;
using Application.Abstractions.Integration.Faktura;
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
    private readonly IFakturaAuthSessionStore _authSessionStore;
    private readonly IUserContext _userContext;
    private readonly ILogger<FakturaAuthorizationHandler> _logger;

    public FakturaAuthorizationHandler(
        IFakturaTokenService tokenService,
        IFakturaAuthSessionStore authSessionStore,
        IUserContext userContext,
        ILogger<FakturaAuthorizationHandler> logger)
    {
        _tokenService = tokenService;
        _authSessionStore = authSessionStore;
        _userContext = userContext;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var session = await TryGetSessionAsync(cancellationToken);
        if (session is not null)
        {
            var cookieHeader = string.Join(
                "; ",
                session.Cookies.Select(cookie => $"{cookie.Name}={cookie.Value}"));
            request.Headers.Remove("Cookie");
            request.Headers.TryAddWithoutValidation("Cookie", cookieHeader);

            _logger.LogDebug(
                "Sending Faktura session request {Method} to host {Host}, path {Path}; sessionCookiePresent={SessionCookiePresent}, authorizationPresent=false, authorizationScheme=none.",
                request.Method.Method,
                request.RequestUri?.Host,
                request.RequestUri?.AbsolutePath,
                !string.IsNullOrWhiteSpace(cookieHeader));

            return await base.SendAsync(request, cancellationToken);
        }

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

    private async Task<FakturaAuthSession?> TryGetSessionAsync(CancellationToken ct)
    {
        if (_userContext.Id is not int userId
            || _userContext.OrganizationId is not int organizationId)
        {
            return null;
        }

        try
        {
            return await _authSessionStore.GetAsync(
                new FakturaAuthSessionScope(userId, organizationId, EdoProviderCode.FAKTURA),
                ct);
        }
        catch (Exception ex) when (ex is IOException
            or UnauthorizedAccessException
            or InvalidOperationException)
        {
            // A corrupt or unavailable session must not expose cookie data and may fall back to OAuth/password.
            return null;
        }
    }

}
