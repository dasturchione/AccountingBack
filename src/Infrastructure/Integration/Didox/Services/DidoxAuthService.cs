using Application.Abstractions.Authentication;
using Application.Features.Integration.Didox.Services;
using Infrastructure.Persistence;
using Integration.Didox.Http;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Exceptions;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Integration.Didox.Services;

public sealed class DidoxAuthService : IDidoxAuthService
{
    private const int MaxErrorBodyBytes = 4096;
    private const int MaxErrorFieldLength = 512;

    // INT_DIDOX.md §2.2: token — UUID, amal muddati 360 daqiqa. Xavfsizlik zaxirasi
    // sifatida biroz oldin yangilanadi (Edocs/AslBelgi'da ham shu naqsh — 5 daqiqa).
    private static readonly TimeSpan TokenLifetime = TimeSpan.FromMinutes(360);

    // INT_DIDOX.md §2.3: "Yo'l parametrlari: taxId/companyTaxId ✅ (ИНН/ПИНФЛ),
    // locale ⬜ (ru default yoki uz)" — hujjatning o'zi TASDIQLAGAN standart qiymat,
    // taxmin emas.
    private const string DefaultLocale = "ru";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly Regex SensitiveValueRegex = new(
        @"(?i)\b(?:pkcs7|signaturehex|signature|private(?:\s|_)?key|partner-authorization|authorization|access[_\s-]?token|auth[_\s-]?token|user[_\s-]?key)\b\s*[:=]\s*(?:""[^"" ]*""|'[^']*'|[^\s,;]+)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private readonly AppDbContext _context;
    private readonly IUserContext _userContext;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly DidoxTokenCache _tokenCache;
    private readonly DidoxTimestampClient _timestampClient;

    public DidoxAuthService(
        AppDbContext context,
        IUserContext userContext,
        IHttpClientFactory httpClientFactory,
        DidoxTokenCache tokenCache,
        DidoxTimestampClient timestampClient)
    {
        _context = context;
        _userContext = userContext;
        _httpClientFactory = httpClientFactory;
        _tokenCache = tokenCache;
        _timestampClient = timestampClient;
    }

    public async Task<DidoxAuthChallengeResultDto> GetAuthChallengeAsync(CancellationToken ct = default)
    {
        var organizationId = RequireOrganization();
        var inn = await RequireOrganizationInnAsync(organizationId, ct);

        var innBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(inn));
        return new DidoxAuthChallengeResultDto { Inn = inn, InnBase64 = innBase64 };
    }

    public async Task<DidoxAuthCompleteResultDto> CompleteAuthAsync(DidoxAuthCompleteRequestDto request, CancellationToken ct = default)
    {
        var organizationId = RequireOrganization();
        var inn = await RequireOrganizationInnAsync(organizationId, ct);

        var client = _httpClientFactory.CreateClient(DidoxHttpClientNames.AuthClient);

        // 1-qadam: POST /v1/dsvs/timestamp — imzoga timestamp biriktiriladi.
        // INT_DIDOX.md §3.1: kirish pkcs7+signatureHex, javob timeStampTokenB64/success/isAttachedPkcs7.
        // Chaqiruv DidoxTimestampClient'ga chiqarilgan (7.3-bosqich) — hujjatni imzolash
        // oqimi ham xuddi shu chaqiruvni ishlatadi, takrorlanmaydi. LOGIN rejimi —
        // user-key hali yo'q (8-bosqich, PROCESS7_AUDIT #3 tuzatishi).
        var timeStampTokenB64 = await _timestampClient.GetTimeStampTokenForAuthAsync(request.Pkcs7, request.SignatureHex, ct);

        // 2-qadam: POST /v1/auth/{taxId}/token/{locale} — ЭЦП orqali token olish.
        // INT_DIDOX.md §2.3 "Usul 1": tanasi {"signature": "<timeStampTokenB64>"}.
        using var tokenResponse = await client.PostAsJsonAsync(
            $"v1/auth/{Uri.EscapeDataString(inn)}/token/{DefaultLocale}",
            new { signature = timeStampTokenB64 },
            JsonOptions,
            ct);

        if (!tokenResponse.IsSuccessStatusCode)
        {
            var providerError = await ReadProviderErrorAsync(tokenResponse, ct);
            throw MapError(tokenResponse.StatusCode, "auth/token", providerError);
        }

        // INT_DIDOX.md §2.2 — javob shakli tasdiqlangan:
        // { "token": "<uuid>", "related_companies": null, "related_branches": null }
        var token = await ExtractStringPropertyAsync(tokenResponse, "token", ct);
        if (string.IsNullOrWhiteSpace(token))
            throw new IntegrationHttpException("Didox auth/token javobida token topilmadi.", 502);

        var expiresAt = DateTimeOffset.UtcNow.Add(TokenLifetime);
        _tokenCache.Set(organizationId, token, TokenLifetime);
        return new DidoxAuthCompleteResultDto
        {
            Success = true,
            ExpiresAt = expiresAt
        };
    }

    private async Task<string> RequireOrganizationInnAsync(int organizationId, CancellationToken ct)
    {
        var inn = await _context.Organizations
            .Where(o => o.Id == organizationId)
            .Select(o => o.Inn)
            .SingleOrDefaultAsync(ct);

        if (string.IsNullOrWhiteSpace(inn))
            throw new InvalidOperationException($"Organization {organizationId} has no INN (org_organization.inn) — Didox login requires it.");

        return inn;
    }

    private static async Task<string?> ExtractStringPropertyAsync(HttpResponseMessage response, string propertyName, CancellationToken ct)
    {
        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

        return document.RootElement.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }

    private static async Task<DidoxProviderError?> ReadProviderErrorAsync(
        HttpResponseMessage response,
        CancellationToken ct)
    {
        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        var buffer = new byte[MaxErrorBodyBytes];
        var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), ct);

        if (bytesRead == 0)
            return null;

        var body = Encoding.UTF8.GetString(buffer, 0, bytesRead);

        try
        {
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;

            return new DidoxProviderError(
                ReadSafeStringProperty(root, "code", "errorCode", "error_code"),
                ReadSafeStringProperty(root, "message"),
                ReadSafeStringProperty(root, "detail"),
                ReadSafeStringProperty(root, "taxId", "tax_id"));
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string? ReadSafeStringProperty(JsonElement root, params string[] names)
    {
        if (root.ValueKind != JsonValueKind.Object)
            return null;

        foreach (var property in root.EnumerateObject())
        {
            if (!names.Any(name => string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
                || (property.Value.ValueKind != JsonValueKind.String
                    && property.Value.ValueKind != JsonValueKind.Number))
            {
                continue;
            }

            var value = property.Value.ValueKind == JsonValueKind.String
                ? property.Value.GetString()
                : property.Value.GetRawText();

            if (string.IsNullOrWhiteSpace(value))
                return null;

            var redacted = SensitiveValueRegex.Replace(value.Trim(), "<redacted>");
            return redacted.Length <= MaxErrorFieldLength
                ? redacted
                : redacted[..MaxErrorFieldLength] + "...";
        }

        return null;
    }

    // INT_DIDOX.md §2.6 — tasdiqlangan xato kodlari (ro'yxatdan o'tish/login uchun,
    // umumiy naqsh sifatida qo'llanildi).
    private static Exception MapError(
        HttpStatusCode statusCode,
        string endpoint,
        DidoxProviderError? providerError) => statusCode switch
    {
        HttpStatusCode.Unauthorized => new IntegrationUnauthorizedException(
            $"Didox {endpoint} so'rovi rad etildi (401) — imzo yaroqsiz.{FormatProviderError(providerError)}"),
        HttpStatusCode.Forbidden => new IntegrationForbiddenException(
            $"Didox {endpoint} so'rovini rad etdi (403).{FormatProviderError(providerError)}"),
        HttpStatusCode.UnprocessableEntity => new IntegrationHttpException(
            $"Didox {endpoint} so'rovi rad etildi (422) — foydalanuvchi ro'yxatdan o'tmagan yoki so'rov yaroqsiz.{FormatProviderError(providerError)}",
            422),
        HttpStatusCode.Locked => new IntegrationHttpException(
            $"Didox {endpoint}: hisob bloklangan (423).{FormatProviderError(providerError)}", 423),
        (HttpStatusCode)429 => new IntegrationHttpException(
            $"Didox {endpoint}: urinishlar juda ko'p (429).{FormatProviderError(providerError)}", 429),
        _ => new IntegrationHttpException(
            $"Didox {endpoint} so'rovi HTTP {(int)statusCode} bilan tugadi.{FormatProviderError(providerError)}",
            (int)statusCode)
    };

    private static string FormatProviderError(DidoxProviderError? providerError)
    {
        if (providerError is null)
            return string.Empty;

        var details = new[]
        {
            providerError.Code is null ? null : $"code={providerError.Code}",
            providerError.Message is null ? null : $"message={providerError.Message}",
            providerError.Detail is null ? null : $"detail={providerError.Detail}",
            providerError.TaxId is null ? null : $"taxId={providerError.TaxId}"
        }.Where(value => value is not null);

        var formatted = string.Join("; ", details);
        return string.IsNullOrWhiteSpace(formatted)
            ? string.Empty
            : $" Provider response: {formatted}";
    }

    private sealed record DidoxProviderError(string? Code, string? Message, string? Detail, string? TaxId);

    private int RequireOrganization() => _userContext.OrganizationId
        ?? throw new InvalidOperationException("An active organization is required for Didox authentication.");
}
