using System.Security.Cryptography;
using System.Text;
using Application.Abstractions.Integration;
using Domain.Entities;
using SharedKernel.Results;

namespace Application.Features.Integration.EImzo.Policies;

/// <summary>
/// Canonicalization policy for E-DOCS. It is compatible with the existing E-DOCS challenge model
/// (see <see cref="EdocsChallengeRecord"/> / <see cref="IEdocsChallengeStore"/>): the holder signs the
/// server-issued authId, so the canonical payload is exactly the authId bytes and the mode is the
/// attached PKCS#7 that the E-DOCS login endpoint already consumes. Nothing here is invented — the
/// contract mirrors what the E-DOCS flow uses today; it is not wired into that flow.
/// </summary>
public sealed class EdocsEImzoSignaturePolicy : IEImzoProviderSignaturePolicy
{
    public Provider Provider => Provider.EDocs;

    public Result<EImzoCanonicalPayload> CreateCanonicalPayload(EImzoCanonicalPayloadRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ChallengeContent))
            return Result.Failure<EImzoCanonicalPayload>(EImzoSignatureRelayErrors.ChallengeContentRequired);

        // The E-DOCS authId is signed as-is; canonical content is its UTF-8 bytes.
        var content = Encoding.UTF8.GetBytes(request.ChallengeContent);
        var payloadHash = Convert.ToBase64String(SHA256.HashData(content));

        return Result.Success(new EImzoCanonicalPayload(content, payloadHash, Pkcs7SignMode.Attached));
    }
}
