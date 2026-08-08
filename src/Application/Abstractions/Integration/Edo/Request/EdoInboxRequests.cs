namespace Application.Abstractions.Integration.Edo;

public sealed class EdoInboxQueryDto
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? Search { get; init; }
    public bool? HasMarks { get; init; }
    public EdoDocumentCategory? Category { get; init; }
    public EdoDocumentStatusCode? Status { get; init; }
    public DateOnly? FromDate { get; init; }
    public DateOnly? ToDate { get; init; }
}

public enum EdoDocumentQueryScope
{
    INBOX,
    OUTBOX,
    ALL
}

public sealed class EdoDocumentQueryDto
{
    public EdoDocumentQueryScope Scope { get; init; } = EdoDocumentQueryScope.INBOX;
    public int Page { get; init; } = 1;
    public int Limit { get; init; } = 20;
    public string? Search { get; init; }
    public bool? HasMarks { get; init; }
    public EdoDocumentCategory? Category { get; init; }
    public EdoDocumentStatusCode? Status { get; init; }
    public DateOnly? DateFrom { get; init; }
    public DateOnly? DateTo { get; init; }

    public IReadOnlyDictionary<string, string?> ProviderFilters { get; init; } =
        new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

    public EdoInboxQueryDto ToInboxQuery() => new()
    {
        Page = Page,
        PageSize = Limit,
        Search = Search,
        HasMarks = HasMarks,
        Category = Category,
        Status = Status,
        FromDate = DateFrom,
        ToDate = DateTo
    };
}

/// <summary>
/// Public query contract for the common outbox endpoint.
/// Provider scope and provider-specific filters stay internal to the adapter request.
/// </summary>
public sealed class EdoOutboxQueryDto
{
    public int Page { get; init; } = 1;
    public int? PageSize { get; init; }
    public string? Search { get; init; }
    public bool? HasMarks { get; init; }
    public EdoDocumentCategory? Category { get; init; }
    public EdoDocumentStatusCode? Status { get; init; }
    public DateOnly? DateFrom { get; init; }
    public DateOnly? DateTo { get; init; }

    // Legacy clients may still send Limit. It is deliberately removed from the
    // public OpenAPI description by EdoPublicContractOperationFilter.
    public int? Limit { get; init; }

    public EdoDocumentQueryDto ToProviderQuery() => new()
    {
        Scope = EdoDocumentQueryScope.OUTBOX,
        Page = Page,
        Limit = PageSize ?? Limit ?? 20,
        Search = Search,
        HasMarks = HasMarks,
        Category = Category,
        Status = Status,
        DateFrom = DateFrom,
        DateTo = DateTo
    };
}

/// <summary>
/// Public query contract for the aggregated all-documents endpoint.
/// Scope, Limit and provider-specific filters remain internal.
/// </summary>
public sealed class EdoAllDocumentsQueryDto
{
    public int Page { get; init; } = 1;
    public int? PageSize { get; init; }
    public string? Search { get; init; }
    public bool? HasMarks { get; init; }
    public EdoDocumentCategory? Category { get; init; }
    public EdoDocumentStatusCode? Status { get; init; }
    public DateOnly? DateFrom { get; init; }
    public DateOnly? DateTo { get; init; }

    internal int EffectivePageSize => PageSize ?? 20;

    public EdoInboxQueryDto ToInboxQuery(int page) => new()
    {
        Page = page,
        PageSize = EffectivePageSize,
        Search = Search,
        HasMarks = HasMarks,
        Category = Category,
        Status = Status,
        FromDate = DateFrom,
        ToDate = DateTo
    };

    public EdoDocumentQueryDto ToOutboxQuery(int page) => new()
    {
        Scope = EdoDocumentQueryScope.OUTBOX,
        Page = page,
        Limit = EffectivePageSize,
        Search = Search,
        HasMarks = HasMarks,
        Category = Category,
        Status = Status,
        DateFrom = DateFrom,
        DateTo = DateTo
    };
}

public sealed class EdoInboxRejectRequestDto
{
    public string Reason { get; init; } = string.Empty;
    public string IdempotencyKey { get; init; } = string.Empty;
    public string? SigningSessionId { get; init; }
    public string? PreparedPkcs7 { get; init; }
    public string? SignatureHex { get; init; }
}
