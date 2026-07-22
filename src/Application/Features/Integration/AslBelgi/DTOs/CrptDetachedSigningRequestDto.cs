namespace Application.Features.Integration.AslBelgi.DTOs;

// This contract is intended for a browser or local E-IMZO client. It must not contain a private key.
public sealed class CrptDetachedSigningRequestDto
{
    public string DocumentBody { get; init; } = string.Empty;
}
