using Application.Abstractions.Authentication;
using Application.Features.Integration.Edocs.Services;
using Integration.Edocs.Http;
using Integration.Shared.Http;
using SharedKernel.Constants;
using SharedKernel.Exceptions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Integration.Edocs.Services;

public sealed class EdocsAuthService : IEdocsAuthService
{
    private const int MaxErrorBodyBytes = 4096;
    private const int MaxErrorFieldLength = 512;
    private const int MaxProviderErrorDepth = 4;
    private const int ExpectedChallengeTtlSeconds = 120;

    // Hujjat: "После успешной авторизации возвращается токен. Срок действия токена
    // 24 часа." Xavfsizlik zaxirasi sifatida biroz oldin yangilanadi (login/parol
    // yo'lida ham xuddi shu qiymat ishlatilgan edi).
    // Edocs tokenining 24 soatlik muddati shartnoma qiymati; konfiguratsiyaga chiqarilmaydi.
    private static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(24) - TimeSpan.FromMinutes(5);

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly Regex SensitiveValueRegex = new(
        @"(?i)[""']?\b(?:pkcs7|signaturehex|signature|private(?:\s|_)?key|partner-authorization|authorization|access[_\s-]?token|auth[_\s-]?token|user[_\s-]?key|token)\b[""']?\s*[:=]\s*(?:""[^"" ]*""|'[^']*'|[^\s,;}\]]+)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

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

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/frontend/challenge?_uc={Guid.NewGuid():N}");
        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

        if (!response.IsSuccessStatusCode)
            throw MapError(response.StatusCode, "challenge");

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
        var root = document.RootElement;

        // ESLATMA: hujjatda /authId javobining aniq JSON shakli ko'rsatilmagan.
        // Ikkita mumkin bo'lgan shakl tekshiriladi; haqiqiy format keyingi bosqichda
        // (haqiqiy ЭЦП bilan) tasdiqlanishi kerak.
        if (root.ValueKind != JsonValueKind.Object
            || !root.TryGetProperty("status", out var statusProperty)
            || statusProperty.ValueKind != JsonValueKind.Number
            || !statusProperty.TryGetInt32(out var status)
            || status != 1)
        {
            throw new IntegrationHttpException("Edocs challenge javobida status 1 emas.", 502);
        }

        if (!root.TryGetProperty("ttl", out var ttlProperty)
            || ttlProperty.ValueKind != JsonValueKind.Number
            || !ttlProperty.TryGetInt32(out var ttl)
            || ttl != ExpectedChallengeTtlSeconds)
        {
            throw new IntegrationHttpException(
                $"Edocs challenge javobidagi ttl mavjud ChallengeTtlSeconds={ExpectedChallengeTtlSeconds} qiymatiga mos emas.",
                502);
        }

        var challenge = root.TryGetProperty("challenge", out var challengeProperty)
            && challengeProperty.ValueKind == JsonValueKind.String
            ? challengeProperty.GetString()
            : null;

        if (string.IsNullOrWhiteSpace(challenge))
            throw new IntegrationHttpException("Edocs challenge javobida challenge topilmadi.", 502);

        return new EdocsAuthChallengeResultDto { AuthId = challenge };
    }

    public async Task<EdocsAuthCompleteResultDto> CompleteAuthAsync(EdocsAuthCompleteRequestDto request, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient(EdocsHttpClientNames.AuthClient);
        var timestampedPkcs7 = await GetTimestampPkcs7Async(client, request.Pkcs7, ct);

        using var loginRequest = new HttpRequestMessage(HttpMethod.Post, "login")
        {
            Content = JsonContent.Create(
                new { serialNumber = request.SerialNumber, pkcs7 = timestampedPkcs7 },
                options: JsonOptions)
        };
        ApplyBrowserAuthenticationHeaders(loginRequest);

        using var response = await client.SendAsync(loginRequest, ct);

        if (!response.IsSuccessStatusCode)
        {
            var providerError = await ReadProviderErrorAsync(response, ct);
            throw MapError(response.StatusCode, "login", providerError);
        }

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
        var token = ExtractToken(document.RootElement);

        if (string.IsNullOrWhiteSpace(token))
            throw new IntegrationHttpException("Edocs login javobida token topilmadi.", 502);

        _tokenCache.Set(RequireOrganization(), IntegrationProviderConst.Edocs, token, TokenLifetime);
        return new EdocsAuthCompleteResultDto { Success = true };
    }

    private static async Task<string> GetTimestampPkcs7Async(
        HttpClient client,
        string pkcs7,
        CancellationToken ct)
    {
        using var timestampRequest = new HttpRequestMessage(HttpMethod.Post, "dsvs/gettimestamp")
        {
            Content = JsonContent.Create(new { pkcs7_64 = pkcs7 }, options: JsonOptions)
        };
        ApplyBrowserAuthenticationHeaders(timestampRequest);

        using var response = await client.SendAsync(timestampRequest, ct);

        if (!response.IsSuccessStatusCode)
        {
            var providerError = await ReadProviderErrorAsync(response, ct);
            throw MapError(response.StatusCode, "dsvs/gettimestamp", providerError);
        }

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
        var root = document.RootElement;

        var success = root.ValueKind == JsonValueKind.Object
            && root.TryGetProperty("success", out var successProperty)
            && successProperty.ValueKind == JsonValueKind.True;

        if (!success)
            throw new IntegrationHttpException("Edocs gettimestamp javobida success true emas.", 502);

        var timestampedPkcs7 = root.TryGetProperty("pkcs7_64", out var pkcs7Property)
            && pkcs7Property.ValueKind == JsonValueKind.String
            ? pkcs7Property.GetString()
            : null;

        if (string.IsNullOrWhiteSpace(timestampedPkcs7))
            throw new IntegrationHttpException("Edocs gettimestamp javobida pkcs7_64 topilmadi.", 502);

        return timestampedPkcs7;
    }

    private static void ApplyBrowserAuthenticationHeaders(HttpRequestMessage request)
    {
        request.Headers.TryAddWithoutValidation("Authorization", "Bearer null");
        request.Headers.TryAddWithoutValidation("Origin", "https://doc.edocs.uz");
        request.Headers.TryAddWithoutValidation("Referer", "https://doc.edocs.uz/");
        request.Headers.TryAddWithoutValidation("Accept", "application/json, text/plain, */*");
        request.Headers.TryAddWithoutValidation(
            "User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/151.0.0.0 Safari/537.36");
        request.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
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

    private static async Task<EdocsProviderError?> ReadProviderErrorAsync(
        HttpResponseMessage response,
        CancellationToken ct)
    {
        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        var buffer = new byte[MaxErrorBodyBytes];
        var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), ct);
        var contentType = response.Content.Headers.ContentType?.ToString();

        if (bytesRead == 0)
        {
            return new EdocsProviderError(
                null,
                null,
                null,
                null,
                null,
                null,
                contentType,
                null);
        }

        var body = Encoding.UTF8.GetString(buffer, 0, bytesRead);

        try
        {
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;

            var code = ReadSafeStringProperty(root, "code");
            var message = ReadSafeStringProperty(root, "message");
            var detail = ReadSafeStringProperty(root, "detail");
            var error = ReadSafeStringProperty(root, "error");
            var reason = ReadSafeStringProperty(root, "reason");
            var status = ReadSafeStringProperty(root, "status");
            var hasProviderFields = code is not null
                || message is not null
                || detail is not null
                || error is not null
                || reason is not null
                || status is not null;

            return new EdocsProviderError(
                code,
                message,
                detail,
                error,
                reason,
                status,
                contentType,
                hasProviderFields ? null : RedactAndTruncate(body));
        }
        catch (JsonException)
        {
            return new EdocsProviderError(
                null,
                null,
                null,
                null,
                null,
                null,
                contentType,
                RedactAndTruncate(body));
        }
    }

    private static string? ReadSafeStringProperty(
        JsonElement root,
        string name,
        int depth = 0)
    {
        if (depth > MaxProviderErrorDepth)
            return null;

        if (root.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in root.EnumerateObject())
            {
                if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    var value = ReadSafeScalar(property.Value);
                    if (value is not null)
                        return value;
                }

                var nestedValue = ReadSafeStringProperty(property.Value, name, depth + 1);
                if (nestedValue is not null)
                    return nestedValue;
            }

            return null;
        }

        if (root.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in root.EnumerateArray())
            {
                var nestedValue = ReadSafeStringProperty(item, name, depth + 1);
                if (nestedValue is not null)
                    return nestedValue;
            }
        }

        return null;
    }

    private static string? ReadSafeScalar(JsonElement value)
    {
        if (value.ValueKind != JsonValueKind.String
            && value.ValueKind != JsonValueKind.Number
            && value.ValueKind != JsonValueKind.True
            && value.ValueKind != JsonValueKind.False)
        {
            return null;
        }

        var rawValue = value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : value.GetRawText();

        if (string.IsNullOrWhiteSpace(rawValue))
            return null;

        var redacted = SensitiveValueRegex.Replace(rawValue.Trim(), "<redacted>");
        return redacted.Length <= MaxErrorFieldLength
            ? redacted
            : redacted[..MaxErrorFieldLength] + "...";
    }

    private static string? RedactAndTruncate(string value)
    {
        var redacted = SensitiveValueRegex.Replace(value, "<redacted>");
        var compact = Regex.Replace(redacted, @"\s+", " ").Trim();

        return string.IsNullOrWhiteSpace(compact)
            ? null
            : compact.Length <= MaxErrorFieldLength
                ? compact
                : compact[..MaxErrorFieldLength] + "...";
    }

    private static Exception MapError(
        HttpStatusCode statusCode,
        string endpoint,
        EdocsProviderError? providerError = null) => statusCode switch
    {
        HttpStatusCode.Unauthorized => new IntegrationUnauthorizedException(
            $"Edocs {endpoint} so'rovi rad etildi (401).{FormatProviderError(providerError)}"),
        HttpStatusCode.Forbidden => new IntegrationForbiddenException(
            $"Edocs {endpoint} so'rovini rad etdi (403).{FormatProviderError(providerError)}"),
        _ => new IntegrationHttpException(
            $"Edocs {endpoint} so'rovi HTTP {(int)statusCode} bilan tugadi.{FormatProviderError(providerError)}",
            (int)statusCode)
    };

    private static string FormatProviderError(EdocsProviderError? providerError)
    {
        if (providerError is null)
            return string.Empty;

        var details = new[]
        {
            providerError.Code is null ? null : $"code={providerError.Code}",
            providerError.Message is null ? null : $"message={providerError.Message}",
            providerError.Detail is null ? null : $"detail={providerError.Detail}",
            providerError.Error is null ? null : $"error={providerError.Error}",
            providerError.Reason is null ? null : $"reason={providerError.Reason}",
            providerError.Status is null ? null : $"status={providerError.Status}",
            providerError.ContentType is null ? null : $"contentType={providerError.ContentType}",
            providerError.BodySummary is null ? null : $"body={providerError.BodySummary}"
        }.Where(value => value is not null);

        var formatted = string.Join("; ", details);
        return string.IsNullOrWhiteSpace(formatted)
            ? string.Empty
            : $" Provider response: {formatted}";
    }

    private sealed record EdocsProviderError(
        string? Code,
        string? Message,
        string? Detail,
        string? Error,
        string? Reason,
        string? Status,
        string? ContentType,
        string? BodySummary);
}
