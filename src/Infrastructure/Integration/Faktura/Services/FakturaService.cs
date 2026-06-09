using Application.Abstractions.Integration;
using Application.Abstractions.Integration.Models;
using Integration.Faktura.Configs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Integration.Faktura.Services;

public class FakturaService : IFakturaService
{
    private readonly IMemoryCache        _cache;
    private readonly HttpClient          _httpClient;
    private readonly FakturaAuthSettings _settings;

    private const string BaseUrl  = "https://api.faktura.uz";
    private const string AuthUrl  = "https://account.faktura.uz/token";
    private const string CacheKey = "FakturaAccessToken";

    public FakturaService(IMemoryCache cache, IOptions<FakturaAuthSettings> options)
    {
        _cache      = cache;
        _settings   = options.Value;
        _httpClient = new HttpClient();
    }

    // ------------------------------------------------------------------ //
    //  Token (cached)
    // ------------------------------------------------------------------ //
    private async Task<string> GetTokenAsync()
    {
        if (_cache.TryGetValue(CacheKey, out string? cached) && cached != null)
            return cached;

        var form = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("grant_type",    _settings.GrantType),
            new KeyValuePair<string, string>("username",      _settings.Username),
            new KeyValuePair<string, string>("password",      _settings.Password),
            new KeyValuePair<string, string>("client_id",     _settings.ClientId),
            new KeyValuePair<string, string>("client_secret", _settings.ClientSecret)
        ]);

        var response = await _httpClient.PostAsync(AuthUrl, form);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Faktura avtorizatsiyasida xatolik: {response.StatusCode}. {error}");
        }

        var json          = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(json);

        if (tokenResponse is null || string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
            throw new InvalidOperationException("Faktura access tokenini olishning imkoni bo'lmadi.");

        _cache.Set(CacheKey, tokenResponse.AccessToken, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(tokenResponse.ExpiresIn - 60)
        });

        return tokenResponse.AccessToken;
    }

    // ------------------------------------------------------------------ //
    //  Company lookup by INN
    // ------------------------------------------------------------------ //
    public async Task<CompanyBasicDetailsDto> GetCompanyDataAsync(string companyInn)
    {
        var token = await GetTokenAsync();

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.GetAsync(
            $"{BaseUrl}/Api/Company/GetCompanyBasicDetails?companyInn={companyInn}");

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Faktura dan kompaniya ma'lumotlarini olishda xatolik: {response.StatusCode}. {error}");
        }

        var json           = await response.Content.ReadAsStringAsync();
        var companyDetails = JsonSerializer.Deserialize<CompanyBasicDetailsDto>(json);

        return companyDetails
            ?? throw new InvalidOperationException("Kompaniya ma'lumotlarini o'qib bo'lmadi.");
    }

    private sealed class TokenResponse
    {
        [JsonPropertyName("access_token")]  public string AccessToken  { get; set; } = null!;
        [JsonPropertyName("token_type")]    public string TokenType    { get; set; } = null!;
        [JsonPropertyName("expires_in")]    public int    ExpiresIn    { get; set; }
        [JsonPropertyName("refresh_token")] public string RefreshToken { get; set; } = null!;
    }
}
