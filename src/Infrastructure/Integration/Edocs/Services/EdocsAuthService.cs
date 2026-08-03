using Application.Abstractions.Authentication;
using Application.Features.Integration.Edocs.Services;
using Integration.Edocs.Http;
using Integration.Shared.Http;
using SharedKernel.Constants;
using SharedKernel.Exceptions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Integration.Edocs.Services;

public sealed class EdocsAuthService : IEdocsAuthService
{
    // Hujjat: "После успешной авторизации возвращается токен. Срок действия токена
    // 24 часа." Xavfsizlik zaxirasi sifatida biroz oldin yangilanadi (login/parol
    // yo'lida ham xuddi shu qiymat ishlatilgan edi).
    // Edocs tokenining 24 soatlik muddati shartnoma qiymati; konfiguratsiyaga chiqarilmaydi.
    private static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(24) - TimeSpan.FromMinutes(5);

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly EdocsTokenCache _tokenCache;
    private readonly IUserContext _userContext;

    public EdocsAuthService(IHttpClientFactory httpClientFactory, EdocsTokenCache tokenCache, IUserContext userContext)
    {
        _httpClientFactory = httpClientFactory;
        _tokenCache = tokenCache;
        _userContext = userContext;
    }

    public async Task<JsonElement> GetProfileAsync(CancellationToken ct = default)
    {
        // EdocsAuthorizationHandler'ning o'qiydigan bucket'i bilan bir xil manba
        // (RequireOrganization → IUserContext.OrganizationId) — CompleteAuthAsync
        // yozgan bucket bilan mos kelishi SHART (6.5.7-bosqich).
        var client = _httpClientFactory.CreateClient(EdocsHttpClientNames.Client);

        using var request = new HttpRequestMessage(HttpMethod.Get, "profile");
        request.Options.Set(IntegrationHttpRequestOptions.OrganizationId, RequireOrganization());
        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

        if (!response.IsSuccessStatusCode)
            throw MapError(response.StatusCode, "profile");

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
        return document.RootElement.Clone();
    }

    public async Task<EdocsAuthChallengeResultDto> GetAuthChallengeAsync(string serialNumber, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(serialNumber))
            throw new InvalidOperationException("serialNumber is required.");

        // Auth handler'siz client — bu chaqiruv tokenni ANIQLASH uchun, unga muhtoj emas.
        var client = _httpClientFactory.CreateClient(EdocsHttpClientNames.AuthClient);

        using var request = new HttpRequestMessage(HttpMethod.Get, $"authId/{Uri.EscapeDataString(serialNumber)}");
        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

        if (!response.IsSuccessStatusCode)
            throw MapError(response.StatusCode, "authId");

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
        var root = document.RootElement;

        // ESLATMA: hujjatda /authId javobining aniq JSON shakli ko'rsatilmagan.
        // Ikkita mumkin bo'lgan shakl tekshiriladi; haqiqiy format keyingi bosqichda
        // (haqiqiy ЭЦП bilan) tasdiqlanishi kerak.
        string? authId = null;
        if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("authId", out var authIdProperty)
            && authIdProperty.ValueKind == JsonValueKind.String)
        {
            authId = authIdProperty.GetString();
        }
        else if (root.ValueKind == JsonValueKind.String)
        {
            authId = root.GetString();
        }

        if (string.IsNullOrWhiteSpace(authId))
            throw new IntegrationHttpException("Edocs authId javobida authId topilmadi.", 502);

        return new EdocsAuthChallengeResultDto { AuthId = authId };
    }

    public async Task<EdocsAuthCompleteResultDto> CompleteAuthAsync(EdocsAuthCompleteRequestDto request, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient(EdocsHttpClientNames.AuthClient);

        using var response = await client.PostAsJsonAsync(
            "login",
            new { authId = request.AuthId, serialNumber = request.SerialNumber, pkcs7 = request.Pkcs7 },
            JsonOptions,
            ct);

        if (!response.IsSuccessStatusCode)
            throw MapError(response.StatusCode, "login");

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
        var token = ExtractToken(document.RootElement);

        if (string.IsNullOrWhiteSpace(token))
            throw new IntegrationHttpException("Edocs login javobida token topilmadi.", 502);

        _tokenCache.Set(RequireOrganization(), IntegrationProviderConst.Edocs, token, TokenLifetime);
        return new EdocsAuthCompleteResultDto { Success = true };
    }

    private int RequireOrganization() => _userContext.OrganizationId
        ?? throw new InvalidOperationException("An active organization is required to complete Edocs authentication.");

    // ESLATMA: hujjatda /login javobining aniq JSON shakli ko'rsatilmagan (faqat
    // "токен qaytariladi" deyilgan) — login/parol yo'lidagi (6.1-bosqich, endi
    // o'chirilgan) bilan bir xil taxmin qilinadi: token/accessToken/yalang'och satr.
    private static string? ExtractToken(JsonElement root)
    {
        if (root.ValueKind == JsonValueKind.Object)
        {
            if (root.TryGetProperty("token", out var tokenProperty) && tokenProperty.ValueKind == JsonValueKind.String)
                return tokenProperty.GetString();

            if (root.TryGetProperty("accessToken", out var accessTokenProperty) && accessTokenProperty.ValueKind == JsonValueKind.String)
                return accessTokenProperty.GetString();
        }
        else if (root.ValueKind == JsonValueKind.String)
        {
            return root.GetString();
        }

        return null;
    }

    private static Exception MapError(HttpStatusCode statusCode, string endpoint) => statusCode switch
    {
        HttpStatusCode.Unauthorized => new IntegrationUnauthorizedException($"Edocs {endpoint} so'rovi rad etildi (401)."),
        HttpStatusCode.Forbidden => new IntegrationForbiddenException($"Edocs {endpoint} so'rovini rad etdi (403)."),
        _ => new IntegrationHttpException($"Edocs {endpoint} so'rovi HTTP {(int)statusCode} bilan tugadi.", (int)statusCode)
    };
}
