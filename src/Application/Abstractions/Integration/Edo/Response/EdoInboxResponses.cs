namespace Application.Abstractions.Integration.Edo;

public sealed class EdoInboxListDto
{
    public IReadOnlyCollection<EdoDocumentDto> Items { get; init; } = [];
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int? TotalCount { get; init; }
}

public sealed class EdoInboxRejectDto
{
    public EdoDocumentDto Document { get; init; } = new();
    public EdoSigningSessionDto? SigningSession { get; init; }
}
