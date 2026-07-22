using Application.Features.Integration.AslBelgi.DTOs;

namespace Application.Features.Integration.AslBelgi.Signing;

// Implement this only in the client application that can access the user's E-IMZO installation.
// The server must neither implement nor register a private-key signing client.
public interface IEImzoSigningClient
{
    Task<CrptSignedDocumentPayloadDto> SignDetachedAsync(CrptDetachedSigningRequestDto request, CancellationToken ct = default);
}
