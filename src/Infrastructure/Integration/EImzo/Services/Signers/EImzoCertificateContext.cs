using Application.Abstractions.Integration;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.X509;

namespace Integration.EImzo.Services.Signers;

internal sealed class EImzoCertificateContext
{
    public required string Alias { get; init; }
    public required X509Certificate Certificate { get; init; }
    public required AsymmetricKeyParameter PrivateKey { get; init; }
    public required IReadOnlyList<X509Certificate> CertificateChain { get; init; }
    public required CertificateInfo CertificateInfo { get; init; }
}
