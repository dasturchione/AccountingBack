using SharedKernel.Results;

namespace Application.Abstractions.Integration;

public sealed class CertificateInfo
{
    public string SubjectName { get; init; } = null!;
    public string IssuerName { get; init; } = null!;
    public DateTime ValidFrom { get; init; }
    public DateTime ValidTo { get; init; }
    public bool IsExpired { get; init; }
    public string SerialNumber { get; init; } = null!;
    public string SignatureAlgorithm { get; init; } = null!;
    public string? Thumbprint { get; init; }
}

public enum Pkcs7SignMode
{
    Attached = 0,
    Detached = 1
}

public interface IEImzoSigner
{
    Result<CertificateInfo> ReadCertificateInfo(CancellationToken ct = default);
    Result<byte[]> SignPkcs7(byte[] data, Pkcs7SignMode mode = Pkcs7SignMode.Attached, CancellationToken ct = default);
}
