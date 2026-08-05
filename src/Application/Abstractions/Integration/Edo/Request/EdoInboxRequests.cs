namespace Application.Abstractions.Integration.Edo;

public sealed class EdoInboxQueryDto
{
    public string CompanyInn { get; init; } = string.Empty;
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? Search { get; init; }
    public bool? HasMarks { get; init; }
    public EdoDocumentStatusCode? Status { get; init; }
    public DateOnly? FromDate { get; init; }
    public DateOnly? ToDate { get; init; }
}

public sealed class EdoInboxRejectRequestDto
{
    public string Reason { get; init; } = string.Empty;
    public string IdempotencyKey { get; init; } = string.Empty;
    public string? SigningSessionId { get; init; }
    public string? PreparedPkcs7 { get; init; }
    public string? SignatureHex { get; init; }
}
