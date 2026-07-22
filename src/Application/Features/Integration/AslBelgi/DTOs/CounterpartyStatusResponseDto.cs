namespace Application.Features.Integration.AslBelgi.DTOs;

public sealed class CounterpartyStatusResponseDto
{
    public string? Tin { get; init; }
    public IReadOnlyDictionary<string, string>? Name { get; init; }
    public IReadOnlyDictionary<string, string>? FullName { get; init; }
    public IReadOnlyCollection<string>? ProductGroups { get; init; }
}
