using Application.Features.Integration.AslBelgi.DTOs;

namespace Application.Features.Integration.AslBelgi.Signing;

// This placeholder is deliberately not registered in server DI. It is only a safe contract fallback.
public sealed class UnavailableEImzoSigningClient : IEImzoSigningClient
{
    public Task<CrptSignedDocumentPayloadDto> SignDetachedAsync(CrptDetachedSigningRequestDto request, CancellationToken ct = default)
        => Task.FromException<CrptSignedDocumentPayloadDto>(new EImzoClientUnavailableException());
}
