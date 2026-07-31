using Integration.Faktura.Configs;
using Integration.Faktura.Dtos;
using Integration.Shared.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Integration.Faktura.Http;

public sealed class FakturaAuthorizationHandler : DelegatingHandler
{
    private const string CacheKeyPrefix = "FakturaAccessToken";
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
        // 6.5.6-bosqich (poydevor): chaqiruvchi hali request.Options ga organizationId
        // qo'ymaydi (keyingi bosqichda, servis darajasidagi Option A ulanishi bilan birga
        // qilinadi). Shu bosqichgacha 0 — "hali tashkilot bo'yicha ajratilmagan" bucket'ini
        // bildiradi; kredensial manbai (IOptions<FakturaOptions>) bu bosqichda o'zgarmaydi.
        var organizationId = request.Options.TryGetValue(IntegrationHttpRequestOptions.OrganizationId, out var orgId) ? orgId : 0;

        var token = await GetTokenAsync(organizationId, cancellationToken);

        // Token faqat shu so'rovga qo'yiladi; DefaultRequestHeaders'ga tegilmaydi.
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        _logger.LogDebug(
            "Sending Faktura request {Method} to host {Host}.",
            request.Method.Method,
            request.RequestUri?.Host);

        return await base.SendAsync(request, cancellationToken);
    }

    private async Task<string> GetTokenAsync(int organizationId, CancellationToken cancellationToken)
    {
        var cacheKey = $"{CacheKeyPrefix}:{organizationId}";

        if (TryGetCachedToken(cacheKey, out var cached))
            return cached;

        await TokenLock.WaitAsync(cancellationToken);
        try
        {
            if (TryGetCachedToken(cacheKey, out cached))
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

            _cache.Set(cacheKey, tokenResponse.AccessToken, TimeSpan.FromSeconds(lifetimeSeconds));

            return tokenResponse.AccessToken;
        }
        finally
        {
            TokenLock.Release();
        }
    }

    private bool TryGetCachedToken(string cacheKey, out string token)
    {
        if (_cache.TryGetValue(cacheKey, out string? cached) && !string.IsNullOrWhiteSpace(cached))
        {
            token = cached;
            return true;
        }

        token = string.Empty;
        return false;
    }
}
