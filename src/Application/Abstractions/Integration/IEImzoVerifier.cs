using SharedKernel.Results;

namespace Application.Abstractions.Integration;

public sealed class Pkcs7VerifyResult
{
    public bool IsValid { get; init; }
    public string? SubjectName { get; init; }
    public string? Pinfl { get; init; }
    public string? Inn { get; init; }
    public DateTime? SignedAt { get; init; }
    public string? SerialNumber { get; init; }
    public string? OcspStatus { get; init; }
    public bool HasTimestamp { get; init; }
    public byte[]? DocumentBytes { get; init; }
    public string? Error { get; init; }
}

public interface IEImzoVerifier
{
    Task<Result<Pkcs7VerifyResult>> VerifyAttachedAsync(byte[] pkcs7, CancellationToken ct = default);
    Task<Result<Pkcs7VerifyResult>> VerifyDetachedAsync(byte[] document, byte[] pkcs7, CancellationToken ct = default);
}
