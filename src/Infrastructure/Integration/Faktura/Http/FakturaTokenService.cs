using Integration.Faktura.Configs;
using Integration.Faktura.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Json;

namespace Integration.Faktura.Http;

public sealed class FakturaTokenService(
    IHttpClientFactory httpClientFactory,
    IOptions<FakturaOptions> options,
    IMemoryCache cache,
    ILogger<FakturaTokenService> logger) : IFakturaTokenService
{
    private const string CacheKey = "FakturaAccessToken";
    private const int TokenExpiryLeewaySeconds = 60;
    private const int MinimumTokenLifetimeSeconds = 30;
    private static readonly SemaphoreSlim TokenLock = new(1, 1);

    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        if (TryGetCachedToken(out var cached))
            return cached;

        await TokenLock.WaitAsync(cancellationToken);
        try
        {
            if (TryGetCachedToken(out cached))
                return cached;

            var settings = options.Value;
            using var form = new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", settings.Username),
                new KeyValuePair<string, string>("password", settings.Password),
                new KeyValuePair<string, string>("client_id", settings.ClientId),
                new KeyValuePair<string, string>("client_secret", settings.ClientSecret)
            ]);

            var authClient = httpClientFactory.CreateClient(FakturaHttpClientNames.AuthClient);
            using var response = await authClient.PostAsync(settings.AuthUrl, form, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError(
                    "Faktura token request failed with HTTP status {StatusCode}.",
                    (int)response.StatusCode);

                throw response.StatusCode switch
                {
                    HttpStatusCode.Unauthorized => new SharedKernel.Exceptions.IntegrationUnauthorizedException(
                        "Faktura credentials were rejected."),
                    HttpStatusCode.Forbidden => new SharedKernel.Exceptions.IntegrationForbiddenException(
                        "Faktura token request was denied."),
                    _ => new SharedKernel.Exceptions.IntegrationHttpException(
                        $"Faktura token request failed with HTTP status {(int)response.StatusCode}.",
                        (int)response.StatusCode)
                };
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<FakturaTokenResponseDto>(cancellationToken);
            if (tokenResponse is null || string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
                throw new SharedKernel.Exceptions.IntegrationHttpException(
                    "Faktura token response did not contain the documented access_token field.",
                    StatusCodes.Status502BadGateway);

            var lifetimeSeconds = Math.Max(
                MinimumTokenLifetimeSeconds,
                tokenResponse.ExpiresIn - TokenExpiryLeewaySeconds);
            cache.Set(CacheKey, tokenResponse.AccessToken, TimeSpan.FromSeconds(lifetimeSeconds));

            return tokenResponse.AccessToken;
        }
        finally
        {
            TokenLock.Release();
        }
    }

    private bool TryGetCachedToken(out string token)
    {
        if (cache.TryGetValue(CacheKey, out string? cached)
            && !string.IsNullOrWhiteSpace(cached))
        {
            token = cached;
            return true;
        }

        token = string.Empty;
        return false;
    }
}
