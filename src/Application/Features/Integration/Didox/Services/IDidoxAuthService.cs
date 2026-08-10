namespace Application.Features.Integration.Didox.Services;

public sealed class DidoxAuthChallengeResultDto
{
    // Joriy tashkilotning INN i (org_organization.inn) — Didox loginda imzolanadigan
    // ma'lumot aynan shu (INT_DIDOX.md §2.3, "Usul 1 — ЭЦП": "birinchi argument — ИНН
    // base64 da").
    public string Inn { get; init; } = string.Empty;
    public string InnBase64 { get; init; } = string.Empty;
}

public sealed class DidoxAuthCompleteRequestDto
{
    // Frontend E-IMZO'dan tayyor holda keladi (create_pkcs7 → pkcs7_64, signature_hex).
    public string Pkcs7 { get; init; } = string.Empty;
    public string SignatureHex { get; init; } = string.Empty;
}

public sealed class DidoxAuthCompleteResultDto
{
    public bool Success { get; init; }
    public DateTimeOffset? ExpiresAt { get; init; }
}

public interface IDidoxAuthService
{
    // 1-qadam: joriy tashkilotning INN ini (base64) qaytaradi — frontend shuni E-IMZO
    // bilan imzolab, /auth/complete ga yuboradi.
    Task<DidoxAuthChallengeResultDto> GetAuthChallengeAsync(CancellationToken ct = default);

    // 2-qadam: imzolangan {pkcs7, signatureHex} ni Didox'ga uzatadi
    // (POST /v1/dsvs/timestamp → POST /v1/auth/{taxId}/token/{locale}).
    // Muvaffaqiyatli bo'lsa, tokenni DidoxTokenCache'ga (tashkilot bo'yicha) yozadi.
    Task<DidoxAuthCompleteResultDto> CompleteAuthAsync(DidoxAuthCompleteRequestDto request, CancellationToken ct = default);
}
