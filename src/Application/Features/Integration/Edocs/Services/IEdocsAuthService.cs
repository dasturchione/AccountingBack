using System.Text.Json;

namespace Application.Features.Integration.Edocs.Services;

public sealed class EdocsAuthChallengeResultDto
{
    public string AuthId { get; init; } = string.Empty;
    public DateTimeOffset? ExpiresAt { get; init; }
}

public sealed class EdocsAuthCompleteRequestDto
{
    public string AuthId { get; init; } = string.Empty;
    public string SerialNumber { get; init; } = string.Empty;
    public string Pkcs7 { get; init; } = string.Empty;
}

public sealed class EdocsAuthCompleteResultDto
{
    public bool Success { get; init; }
}

public interface IEdocsAuthService
{
    // Eng past xavfli tekshiruv nuqtasi: mavjud tokenni ishlatadi va GET /profile ni
    // chaqiradi. Faqat o'qiydi, hech narsa yozmaydi.
    Task<JsonElement> GetProfileAsync(CancellationToken ct = default);

    // 1-qadam: GET /authId/{serialNumber} ni proksi qiladi. Qaytgan authId'ni frontend
    // ЭЦП bilan imzolab, /auth/complete ga yuboradi.
    Task<EdocsAuthChallengeResultDto> GetAuthChallengeAsync(string serialNumber, CancellationToken ct = default);

    // 2-qadam: imzolangan {authId, serialNumber, pkcs7} ni POST /login ga uzatadi.
    // Muvaffaqiyatli bo'lsa, tokenni EdocsTokenCache'ga yozadi.
    Task<EdocsAuthCompleteResultDto> CompleteAuthAsync(EdocsAuthCompleteRequestDto request, CancellationToken ct = default);
}
