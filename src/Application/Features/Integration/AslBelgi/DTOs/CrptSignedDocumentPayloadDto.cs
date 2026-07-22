namespace Application.Features.Integration.AslBelgi.DTOs;

public sealed class CrptSignedDocumentPayloadDto
{
    public string DocumentBody { get; init; } = string.Empty;
    public string Signature { get; init; } = string.Empty;
}
