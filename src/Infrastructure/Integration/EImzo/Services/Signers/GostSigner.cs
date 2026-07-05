using Application.Abstractions.Integration;
using Microsoft.Extensions.Logging;
using SharedKernel.Results;

namespace Integration.EImzo.Services.Signers;

internal sealed class GostSigner : ICertificateSigner
{
    private readonly ILogger<GostSigner> _logger;

    public GostSigner(ILogger<GostSigner> logger)
    {
        _logger = logger;
    }

    public bool CanSign(EImzoSignatureAlgorithm signatureAlgorithm)
    {
        return signatureAlgorithm.Oid?.StartsWith("1.2.860.", StringComparison.Ordinal) == true
            || signatureAlgorithm.Name?.Contains("OZDST", StringComparison.OrdinalIgnoreCase) == true
            || signatureAlgorithm.Name?.Contains("GOST", StringComparison.OrdinalIgnoreCase) == true;
    }

    public Result<byte[]> Sign(byte[] data, EImzoCertificateContext certificateContext, Pkcs7SignMode mode, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        if (data.Length == 0)
            return Result.Failure<byte[]>(Error.Problem("EImzo.SignDataMissing", "PKCS#7 signing payload is empty."));

        // Extension point: when the official Uzbekistan E-IMZO SDK/CSP is available,
        // replace this failure path with the SDK-backed PKCS#7 implementation without
        // changing EImzoSigner or other registered strategies.
        var algorithm = EImzoSignatureAlgorithm.Parse(certificateContext.CertificateInfo.SignatureAlgorithm);

        _logger.LogInformation(
            "OZDST signing was requested for algorithm {SignatureAlgorithm}, but the SDK/CSP-backed signer is not installed",
            algorithm.DisplayValue);

        return Result.Failure<byte[]>(Error.Problem(
            "EImzo.GostSdkRequired",
            "O'zbekiston E-IMZO (OZDST/1.2.860.*) backend signing uchun rasmiy SDK yoki CSP/provider kerak. Hozirgi BouncyCastle bu profilni qamramaydi. Real imzolash: (1) desktop E-IMZO client, (2) Didox provider, yoki (3) O'zbekiston E-IMZO SDK orqali."));
    }
}
