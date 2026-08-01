namespace Application.Abstractions.Integration.Edo;

public sealed class EdoAuthChallengeRequestDto
{
    public string? CertificateSerialNumber { get; init; }
    public EdoAuthMode? AuthMode { get; init; }
}

public sealed class EdoAuthChallengeDto
{
    public string ChallengeId { get; init; } = string.Empty;
    public EdoAuthMode AuthMode { get; init; }
    public string Payload { get; init; } = string.Empty;
    public string PayloadFormat { get; init; } = string.Empty;
    public DateTimeOffset? ExpiresAt { get; init; }
    public string? SigningSessionId { get; init; }
}

public sealed class EdoAuthCompleteRequestDto
{
    public string ChallengeId { get; init; } = string.Empty;
    public string? SigningSessionId { get; init; }
    public string? CertificateSerialNumber { get; init; }
    public string? SignedPayload { get; init; }
    public string? PreparedPkcs7 { get; init; }
    public string? SignatureHex { get; init; }
}

public sealed class EdoAuthCompleteDto
{
    public bool IsAuthenticated { get; init; }
    public string? SessionId { get; init; }
    public DateTimeOffset? ExpiresAt { get; init; }
}

public sealed class EdoSigningSessionDto
{
    public string SessionId { get; init; } = string.Empty;
    public EdoSigningMode SigningMode { get; init; }
    public string? DocumentId { get; init; }
    public string? Payload { get; init; }
    public string? PayloadFormat { get; init; }
    public DateTimeOffset? ExpiresAt { get; init; }
}
