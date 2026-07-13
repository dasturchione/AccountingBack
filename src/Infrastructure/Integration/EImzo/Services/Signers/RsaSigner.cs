using Application.Abstractions.Integration;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Cms;
using SharedKernel.Results;

namespace Integration.EImzo.Services.Signers;

internal sealed class RsaSigner : ICertificateSigner
{
    private readonly ILogger<RsaSigner> _logger;

    public RsaSigner(ILogger<RsaSigner> logger)
    {
        _logger = logger;
    }

    public bool CanSign(EImzoSignatureAlgorithm signatureAlgorithm)
    {
        return signatureAlgorithm.Oid?.StartsWith("1.2.840.113549.1.1.", StringComparison.Ordinal) == true
            || signatureAlgorithm.Name?.Contains("RSA", StringComparison.OrdinalIgnoreCase) == true;
    }

    public Result<byte[]> Sign(byte[] data, EImzoCertificateContext certificateContext, Pkcs7SignMode mode, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        if (data.Length == 0)
            return Result.Failure<byte[]>(Error.Problem("EImzo.SignDataMissing", "PKCS#7 signing payload is empty."));

        try
        {
            var generator = new CmsSignedDataGenerator();

            foreach (var certificate in certificateContext.CertificateChain.Append(certificateContext.Certificate).DistinctBy(x => x.SerialNumber.ToString(16)))
            {
                generator.AddCertificate(certificate);
            }

            generator.AddSigner(
                certificateContext.PrivateKey,
                certificateContext.Certificate,
                ResolveDigestOid(certificateContext.CertificateInfo.SignatureAlgorithm));

            var cms = generator.Generate(new CmsProcessableByteArray(data), mode == Pkcs7SignMode.Attached);

            _logger.LogInformation(
                "PKCS#7 signature generated with RSA signer in {Mode} mode using {SignatureAlgorithm}",
                mode,
                certificateContext.CertificateInfo.SignatureAlgorithm);

            return Result.Success(cms.GetEncoded());
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                "RSA PKCS#7 signing failed for algorithm {SignatureAlgorithm}; exception type {ExceptionType}",
                certificateContext.CertificateInfo.SignatureAlgorithm,
                ex.GetType().Name);
            return Result.Failure<byte[]>(Error.Problem("EImzo.RsaSigningFailed", "RSA PKCS#7 signing failed."));
        }
    }

    private static string ResolveDigestOid(string signatureAlgorithm)
    {
        if (signatureAlgorithm.Contains("SHA512", StringComparison.OrdinalIgnoreCase))
            return "2.16.840.1.101.3.4.2.3";

        if (signatureAlgorithm.Contains("SHA384", StringComparison.OrdinalIgnoreCase))
            return "2.16.840.1.101.3.4.2.2";

        if (signatureAlgorithm.Contains("SHA1", StringComparison.OrdinalIgnoreCase))
            return "1.3.14.3.2.26";

        return "2.16.840.1.101.3.4.2.1";
    }
}
