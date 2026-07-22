namespace Application.Features.Integration.AslBelgi.DTOs;

public sealed class MarkingCodeCheckRequestDto
{
    public IReadOnlyCollection<string> Codes { get; init; } = Array.Empty<string>();
}
