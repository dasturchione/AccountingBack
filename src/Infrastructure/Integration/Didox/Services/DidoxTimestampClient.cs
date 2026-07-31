using Integration.Didox.Http;
using Integration.Shared.Http;
using SharedKernel.Exceptions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Integration.Didox.Services;

// INT_DIDOX.md §7.1/§7.2/§3.1: POST /v1/dsvs/timestamp — pkcs7+signatureHex dan
// timeStampTokenB64 olish. 7.1-bosqichda bu chaqiruv DidoxAuthService.CompleteAuthAsync
// ichida edi (login imzosi uchun); 7.3-bosqichda hujjat imzolash oqimiga ham xuddi
// shu chaqiruv kerak bo'ldi — shuning uchun YAGONA joyga chiqarilgan.
//
// INT_DIDOX.md §2.1: "user-key — ✅ (auth dan keyingi barcha so'rovlarda)". Bu chaqiruv
// ikki mutlaqo boshqa kontekstda ishlatiladi:
//   - LOGIN paytida (DidoxAuthService.CompleteAuthAsync) — user-key HALI YO'Q (bu
//     chaqiruv aynan shu tokenni olish jarayonining bir qismi) — faqat
//     Partner-Authorization bilan, AuthClient orqali.
//   - IMZOLASH paytida (DidoxFacturaService.SignFacturaDocumentAsync) — tashkilot
//     ALLAQACHON autentifikatsiyadan o'tgan, §2.1 qoidasiga ko'ra user-key bu yerda
//     ham bo'lishi kerak — asosiy Client (DidoxAuthorizationHandler bilan) orqali.
// 8-bosqich (PROCESS7_AUDIT #3): bu ikki holat endi ATAYLAB ikkita alohida, aniq
// nomlangan metodga ajratildi — chaqiruvchi qaysi rejim kerakligini nom orqali aniq
// ko'rsatadi, birontasi ham "sukut" emas.
public sealed class DidoxTimestampClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IHttpClientFactory _httpClientFactory;

    public DidoxTimestampClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    // LOGIN uchun — user-key hali mavjud emas, faqat Partner-Authorization
    // (AuthClient, DidoxAuthorizationHandler ulanmagan).
    public async Task<string> GetTimeStampTokenForAuthAsync(string pkcs7, string signatureHex, CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient(DidoxHttpClientNames.AuthClient);

        using var response = await client.PostAsJsonAsync(
            "v1/dsvs/timestamp",
            new { pkcs7, signatureHex },
            JsonOptions,
            ct);

        return await ExtractTokenOrThrowAsync(response, ct);
    }

    // HUJJATNI IMZOLASH uchun — tashkilot allaqachon autentifikatsiyadan o'tgan,
    // §2.1 bo'yicha user-key ham yuborilishi kerak (asosiy Client,
    // DidoxAuthorizationHandler orqali — Partner-Authorization + user-key ikkalasi
    // ham avtomatik qo'yiladi).
    public async Task<string> GetTimeStampTokenForSigningAsync(int organizationId, string pkcs7, string signatureHex, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "v1/dsvs/timestamp")
        {
            Content = JsonContent.Create(new { pkcs7, signatureHex }, options: JsonOptions)
        };
        request.Options.Set(IntegrationHttpRequestOptions.OrganizationId, organizationId);

        var client = _httpClientFactory.CreateClient(DidoxHttpClientNames.Client);
        using var response = await client.SendAsync(request, ct);

        return await ExtractTokenOrThrowAsync(response, ct);
    }

    private static async Task<string> ExtractTokenOrThrowAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (!response.IsSuccessStatusCode)
            throw MapError(response.StatusCode);

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

        var token = document.RootElement.TryGetProperty("timeStampTokenB64", out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;

        if (string.IsNullOrWhiteSpace(token))
            throw new IntegrationHttpException("Didox timestamp javobida timeStampTokenB64 topilmadi.", 502);

        return token;
    }

    // INT_DIDOX.md §2.6 — tasdiqlangan xato kodlari (umumiy naqsh, DidoxAuthService
    // dagi asl MapError bilan bir xil qiymatlar).
    private static Exception MapError(HttpStatusCode statusCode) => statusCode switch
    {
        HttpStatusCode.Unauthorized => new IntegrationUnauthorizedException("Didox dsvs/timestamp so'rovi rad etildi (401) — imzo yaroqsiz."),
        HttpStatusCode.Forbidden => new IntegrationForbiddenException("Didox dsvs/timestamp so'rovini rad etdi (403)."),
        HttpStatusCode.UnprocessableEntity => new IntegrationHttpException("Didox dsvs/timestamp so'rovi rad etildi (422).", 422),
        HttpStatusCode.Locked => new IntegrationHttpException("Didox dsvs/timestamp: hisob bloklangan (423).", 423),
        (HttpStatusCode)429 => new IntegrationHttpException("Didox dsvs/timestamp: urinishlar juda ko'p (429).", 429),
        _ => new IntegrationHttpException($"Didox dsvs/timestamp so'rovi HTTP {(int)statusCode} bilan tugadi.", (int)statusCode)
    };
}
