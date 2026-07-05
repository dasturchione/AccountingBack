using Application.Abstractions.Integration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Integration.EImzo.Configs;
using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Pkcs;
using SharedKernel.Results;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace UnitTests;

public sealed class EImzoSignerTests
{
    [Fact]
    public void SignPkcs7_ShouldGenerateAttachedRsaSignature()
    {
        using var tempCertificate = CreateTemporaryCertificate();
        using var signer = CreateSigner(tempCertificate.Path, tempCertificate.Password);
        var payload = "invoice-payload-123"u8.ToArray();

        var result = signer.SignPkcs7(payload, Pkcs7SignMode.Attached);

        Assert.True(result.IsSuccess, result.IsSuccess ? string.Empty : result.Error.Description);

        using var signatureStream = new MemoryStream(result.Value);
        var cms = new CmsSignedData(signatureStream);

        AssertSignerVerification(cms, tempCertificate.Path, tempCertificate.Password);
    }

    [Fact]
    public void SignPkcs7_ShouldGenerateDetachedRsaSignature()
    {
        using var tempCertificate = CreateTemporaryCertificate();
        using var signer = CreateSigner(tempCertificate.Path, tempCertificate.Password);
        var payload = "invoice-payload-456"u8.ToArray();

        var result = signer.SignPkcs7(payload, Pkcs7SignMode.Detached);

        Assert.True(result.IsSuccess, result.IsSuccess ? string.Empty : result.Error.Description);

        using var signatureStream = new MemoryStream(result.Value);
        var cms = new CmsSignedData(new CmsProcessableByteArray(payload), signatureStream);

        AssertSignerVerification(cms, tempCertificate.Path, tempCertificate.Password);
    }

    private static ScopedSigner CreateSigner(string certificatePath, string certificatePassword)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["EImzo:CertificatePath"] = certificatePath,
                ["EImzo:CertificatePassword"] = certificatePassword,
                ["EImzo:ValidateCertificateChain"] = bool.FalseString
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddEImzoIntegration(configuration);

        var serviceProvider = services.BuildServiceProvider();
        var signer = serviceProvider.GetRequiredService<IEImzoSigner>();

        return new ScopedSigner(serviceProvider, signer);
    }

    private static void AssertSignerVerification(CmsSignedData cms, string certificatePath, string certificatePassword)
    {
        var signers = cms.GetSignerInfos().GetSigners().Cast<SignerInformation>().ToList();
        var certificate = LoadSigningCertificate(certificatePath, certificatePassword);

        Assert.NotEmpty(signers);

        foreach (var signer in signers)
        {
            Assert.True(signer.Verify(certificate));
        }
    }

    private static Org.BouncyCastle.X509.X509Certificate LoadSigningCertificate(string certificatePath, string certificatePassword)
    {
        using var stream = File.OpenRead(certificatePath);
        var store = new Pkcs12StoreBuilder().Build();
        store.Load(stream, certificatePassword.ToCharArray());

        var alias = store.Aliases.Cast<string>().First(store.IsKeyEntry);
        return store.GetCertificate(alias).Certificate;
    }

    private static TemporaryCertificate CreateTemporaryCertificate()
    {
        const string password = "UnitTest-Pfx-Password-123!";
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.pfx");

        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest(
            "CN=EImzo Unit Test",
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

    private sealed class ScopedSigner : IEImzoSigner, IDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly IEImzoSigner _innerSigner;

        public ScopedSigner(ServiceProvider serviceProvider, IEImzoSigner innerSigner)
        {
            _serviceProvider = serviceProvider;
            _innerSigner = innerSigner;
        }

        public Result<CertificateInfo> ReadCertificateInfo(CancellationToken ct = default) =>
            _innerSigner.ReadCertificateInfo(ct);

        public Result<byte[]> SignPkcs7(byte[] data, Pkcs7SignMode mode = Pkcs7SignMode.Attached, CancellationToken ct = default) =>
            _innerSigner.SignPkcs7(data, mode, ct);

        public void Dispose()
        {
            _serviceProvider.Dispose();
        }
    }
}
