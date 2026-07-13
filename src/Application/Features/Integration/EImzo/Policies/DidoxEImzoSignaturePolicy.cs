using Application.Abstractions.Integration;
using Domain.Entities;
using SharedKernel.Results;

namespace Application.Features.Integration.EImzo.Policies;

/// <summary>
/// Placeholder policy for Didox. Didox's canonical payload format and PKCS#7 mode are not defined for
/// this relay yet, so it fails closed with <c>UnsupportedContract</c> rather than inventing a format.
/// </summary>
public sealed class DidoxEImzoSignaturePolicy : IEImzoProviderSignaturePolicy
{
    public Provider Provider => Provider.Didox;

    public Result<EImzoCanonicalPayload> CreateCanonicalPayload(EImzoCanonicalPayloadRequest request) =>
        Result.Failure<EImzoCanonicalPayload>(EImzoSignatureRelayErrors.UnsupportedContract);
}
