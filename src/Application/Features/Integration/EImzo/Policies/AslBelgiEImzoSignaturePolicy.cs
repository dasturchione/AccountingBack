using Application.Abstractions.Integration;
using Domain.Entities;
using SharedKernel.Results;

namespace Application.Features.Integration.EImzo.Policies;

/// <summary>
/// Placeholder policy for Asl Belgisi. Asl Belgisi uses business API-key signing; this relay does not
/// assume that signing backend is connected, so it fails closed with <c>UnsupportedContract</c>.
/// </summary>
public sealed class AslBelgiEImzoSignaturePolicy : IEImzoProviderSignaturePolicy
{
    public Provider Provider => Provider.AslBelgi;

    public Result<EImzoCanonicalPayload> CreateCanonicalPayload(EImzoCanonicalPayloadRequest request) =>
        Result.Failure<EImzoCanonicalPayload>(EImzoSignatureRelayErrors.UnsupportedContract);
}
