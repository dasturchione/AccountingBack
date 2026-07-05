using Application.Abstractions.Integration;
using Integration.EImzo.Services.Signers;
using Microsoft.Extensions.Logging.Abstractions;
using Org.BouncyCastle.Pkcs;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace UnitTests;

public sealed class EImzoStrategyTests
{
    [Fact]
    public void SignatureAlgorithm_Parse_ShouldExtractNameAndOid()
    {
        var algorithm = EImzoSignatureAlgorithm.Parse("sha256RSA (1.2.840.113549.1.1.11)");

        Assert.Equal("sha256RSA", algorithm.Name);
        Assert.Equal("1.2.840.113549.1.1.11", algorithm.Oid);
        Assert.Equal("1.2.840.113549.1.1.11", algorithm.DisplayValue);
    }

    [Fact]
    public void RsaSigner_CanSign_ShouldUseOidMatching()
    {
        var signer = new RsaSigner(NullLogger<RsaSigner>.Instance);
        var algorithm = EImzoSignatureAlgorithm.Parse("sha256RSA (1.2.840.113549.1.1.11)");

        Assert.True(signer.CanSign(algorithm));
    }

    [Fact]
    public void GostSigner_CanSign_ShouldRecognizeUzbekistanOid()
    {
        var signer = new GostSigner(NullLogger<GostSigner>.Instance);
        var algorithm = EImzoSignatureAlgorithm.Parse("1.2.860.3.15.1.1.2.2.2.2");

        Assert.True(signer.CanSign(algorithm));
    }

    [Fact]
    public void GostSigner_Sign_ShouldReturnSdkRequiredFailure_ForUzbekistanOid()
    {
        using var tempCertificate = CreateTemporaryCertificate();
        var context = CreateCertificateContext(tempCertificate.Path, tempCertificate.Password, "1.2.860.3.15.1.1.2.2.2.2");
        var signer = new GostSigner(NullLogger<GostSigner>.Instance);

        var result = signer.Sign("test"u8.ToArray(), context, Pkcs7SignMode.Attached);

        Assert.False(result.IsSuccess);
        Assert.Equal("EImzo.GostSdkRequired", result.Error.Code);
    }

    private static EImzoCertificateContext CreateCertificateContext(string certificatePath, string certificatePassword, string signatureAlgorithm)
    {
        using var stream = File.OpenRead(certificatePath);
        var store = new Pkcs12StoreBuilder().Build();
        store.Load(stream, certificatePassword.ToCharArray());

        var alias = store.Aliases.Cast<string>().First(store.IsKeyEntry);
        var certificateEntry = store.GetCertificate(alias);
        var keyEntry = store.GetKey(alias);

        return new EImzoCertificateContext
        {
            Alias = alias,
            Certificate = certificateEntry.Certificate,
            PrivateKey = keyEntry.Key,
            CertificateChain = [certificateEntry.Certificate],
            CertificateInfo = new CertificateInfo
            {
                SubjectName = certificateEntry.Certificate.SubjectDN.ToString(),
                IssuerName = certificateEntry.Certificate.IssuerDN.ToString(),
                ValidFrom = certificateEntry.Certificate.NotBefore,
                ValidTo = certificateEntry.Certificate.NotAfter,
                IsExpired = false,
                SerialNumber = certificateEntry.Certificate.SerialNumber.ToString(16).ToUpperInvariant(),
                SignatureAlgorithm = signatureAlgorithm,
                Thumbprint = Convert.ToHexString(SHA1.HashData(certificateEntry.Certificate.GetEncoded()))
            }
        };
    }

    private static TemporaryCertificate CreateTemporaryCertificate()
    {
        const string password = "UnitTest-Pfx-Password-123!";
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.pfx");

        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest(
            "CN=EImzo Strategy Test",
            rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
        request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, false));
        request.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(request.PublicKey, false));

        using var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(7));
        var pfxBytes = certificate.Export(X509ContentType.Pkcs12, password);
        File.WriteAllBytes(path, pfxBytes);

        return new TemporaryCertificate(path, password);
    }

    private sealed class TemporaryCertificate : IDisposable
    {
        public TemporaryCertificate(string path, string password)
        {
            Path = path;
            Password = password;
        }

        public string Path { get; }
        public string Password { get; }

        public void Dispose()
        {
            if (File.Exists(Path))
                File.Delete(Path);
        }
    }
}
