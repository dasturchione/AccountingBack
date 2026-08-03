namespace Application.Abstractions.Integration.Edo;

public sealed class EdoOutboxCreateDto
{
    public EdoDocumentDto Document { get; init; } = new();
    public EdoSigningSessionDto? SigningSession { get; init; }
    public bool IsReplay { get; init; }
}

public sealed class EdoOutboxSignDto
{
    public EdoDocumentDto Document { get; init; } = new();
    public EdoSigningSessionDto? SigningSession { get; init; }
}
