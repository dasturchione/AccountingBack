using Application.Abstractions.Integration;
using SharedKernel.Results;

namespace Integration.EImzo.Services.Signers;

internal interface ICertificateSigner
{
    // New SDK/CSP-backed signers plug into this contract and DI registration only.
    bool CanSign(EImzoSignatureAlgorithm signatureAlgorithm);
    Result<byte[]> Sign(byte[] data, EImzoCertificateContext certificateContext, Pkcs7SignMode mode, CancellationToken ct = default);
}
