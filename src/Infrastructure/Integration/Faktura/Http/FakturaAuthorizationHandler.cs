using Integration.Faktura.Configs;
using Integration.Faktura.Dtos;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Integration.Faktura.Http;

// 6.5.7-bosqichda tasdiqlandi: Faktura kredensiali (Username/Password/ClientId/ClientSecret)
// tashkilotga tegishli EMAS — bu integratsiyaning yagona chaqiruvchisi
// (OrganizationService.GetByInnAsync, [AllowAnonymous] "by-inn" endpoint) tashkilot hali
// yaratilmasdan turib ishlaydi. Shuning uchun bu handler organizationId/
// IIntegrationCredentialProvider dan FOYDALANMAYDI — kredensial platforma darajasida,
// IOptions<FakturaOptions> orqali, bitta umumiy token keshi bilan qoladi.
public sealed class FakturaAuthorizationHandler : DelegatingHandler
{
    private const string CacheKey = "FakturaAccessToken";
    private const int TokenExpiryLeewaySeconds = 60;
    private const int MinimumTokenLifetimeSeconds = 30;

    // Handler transient bo'lgani uchun qulf static: bir vaqtda kelgan
    // so'rovlar tokenni bir necha marta olmasligi kerak.
    private static readonly SemaphoreSlim TokenLock = new(1, 1);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptions<FakturaOptions> _options;
    private readonly IMemoryCache _cache;
    private readonly ILogger<FakturaAuthorizationHandler> _logger;

    public FakturaAuthorizationHandler(
        IHttpClientFactory httpClientFactory,
        IOptions<FakturaOptions> options,
        IMemoryCache cache,
        ILogger<FakturaAuthorizationHandler> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options;
        _cache = cache;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await GetTokenAsync(cancellationToken);

        // Token faqat shu so'rovga qo'yiladi; DefaultRequestHeaders'ga tegilmaydi.
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        _logger.LogDebug(
            "Sending Faktura request {Method} to host {Host}.",
            request.Method.Method,
            request.RequestUri?.Host);

        return await base.SendAsync(request, cancellationToken);
    }

    private async Task<string> GetTokenAsync(CancellationToken cancellationToken)
    {
        if (TryGetCachedToken(out var cached))
            return cached;

        await TokenLock.WaitAsync(cancellationToken);
        try
        {
            if (TryGetCachedToken(out cached))
                return cached;

            var options = _options.Value;

            using var form = new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("grant_type", options.GrantType),
                new KeyValuePair<string, string>("username", options.Username),
                new KeyValuePair<string, string>("password", options.Password),
                new KeyValuePair<string, string>("client_id", options.ClientId),
                new KeyValuePair<string, string>("client_secret", options.ClientSecret)
            ]);

            var authClient = _httpClientFactory.CreateClient(FakturaHttpClientNames.AuthClient);
            using var response = await authClient.PostAsync(options.AuthUrl, form, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                // Javob tanasi ataylab qo'shilmaydi: bu endpointga login, parol va
                // client_secret yuboriladi, provayder javobi ularni qaytarishi mumkin.
                _logger.LogError(
                    "Faktura token request failed with HTTP status {StatusCode}.",
                    (int)response.StatusCode);

                throw new InvalidOperationException(
                    $"Faktura avtorizatsiyasida xatolik: HTTP {(int)response.StatusCode}.");
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<FakturaTokenResponseDto>(cancellationToken);

            if (tokenResponse is null || string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
                throw new InvalidOperationException("Faktura access tokenini olishning imkoni bo'lmadi.");

            var lifetimeSeconds = Math.Max(
                MinimumTokenLifetimeSeconds,
                tokenResponse.ExpiresIn - TokenExpiryLeewaySeconds);

            _cache.Set(CacheKey, tokenResponse.AccessToken, TimeSpan.FromSeconds(lifetimeSeconds));

            return tokenResponse.AccessToken;
        }
        finally
        {
            TokenLock.Release();
        }
    }

    private bool TryGetCachedToken(out string token)
    {
        if (_cache.TryGetValue(CacheKey, out string? cached) && !string.IsNullOrWhiteSpace(cached))
        {
            token = cached;
            return true;
        }

        token = string.Empty;
        return false;
    }
}
