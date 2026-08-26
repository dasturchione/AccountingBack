namespace Application.Features.Integration.Edo.UnifiedImport;

public sealed class EdoUnifiedImportPlanRequestDto
{
    public IReadOnlyCollection<string> ProviderDocumentIds { get; init; } = [];
    public bool AllowSentDocuments { get; init; }
    public bool AllowUnmatchedMarkings { get; init; }
}
