namespace Application.Abstractions.Integration.Edo;

public sealed class EdoInboxQueryDto
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? Search { get; init; }
    public EdoDocumentStatusCode? Status { get; init; }
    public DateOnly? FromDate { get; init; }
    public DateOnly? ToDate { get; init; }
}

public sealed class EdoInboxListDto
{
    public IReadOnlyCollection<EdoDocumentDto> Items { get; init; } = [];
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int? TotalCount { get; init; }
}

public sealed class EdoInboxRejectRequestDto
{
    public string Reason { get; init; } = string.Empty;
    public string IdempotencyKey { get; init; } = string.Empty;
    public string? SigningSessionId { get; init; }
    public string? PreparedPkcs7 { get; init; }
    public string? SignatureHex { get; init; }
}

public sealed class EdoInboxRejectDto
{
    public EdoDocumentDto Document { get; init; } = new();
    public EdoSigningSessionDto? SigningSession { get; init; }
}
