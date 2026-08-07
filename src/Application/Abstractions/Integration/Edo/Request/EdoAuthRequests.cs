namespace Application.Abstractions.Integration.Edo;

public sealed class EdoAuthChallengeRequestDto
{
    public string? CertificateSerialNumber { get; init; }
    public EdoAuthMode? AuthMode { get; init; }
}

public sealed class EdoAuthCompleteRequestDto
{
    public string ChallengeId { get; init; } = string.Empty;
    public string? SigningSessionId { get; init; }
    public string? CertificateSerialNumber { get; init; }
    public string? PreparedPkcs7 { get; init; }
    public string? SignedPayload { get; init; }
    public string? SignatureHex { get; init; }
}
