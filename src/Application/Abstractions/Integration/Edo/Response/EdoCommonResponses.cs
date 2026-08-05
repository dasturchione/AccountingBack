namespace Application.Abstractions.Integration.Edo;

[System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
public enum EdoDirection
{
    INBOX,
    OUTBOX
}

[System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
public enum EdoDocumentStatusCode
{
    UNKNOWN,
    DRAFT,
    PENDING,
    SIGNED,
    SENT,
    RECEIVED,
    REJECTED,
    COMPLETED,
    CANCELLED,
    FAILED,
    RECONCILIATION_REQUIRED
}

public sealed class EdoDocumentStatusDto
{
    public EdoDocumentStatusCode Code { get; init; }
    public EdoDocumentStatusCode? LocalCode { get; init; }
    public string? ProviderStatusCode { get; init; }
    public string? Description { get; init; }
    public bool IsTerminal { get; init; }
    public bool IsSuccessful { get; init; }
    public DateTimeOffset? CheckedAt { get; init; }
    public bool IsReconciliationRequired { get; init; }
}

public sealed class EdoPartyDto
{
    public string Name { get; init; } = string.Empty;
    public string TaxIdentifier { get; init; } = string.Empty;
    public string? BankCode { get; init; }
    public string? AccountNumber { get; init; }
    public string? Address { get; init; }
    public string? BranchCode { get; init; }
    public string? BranchName { get; init; }
    public string? DirectorName { get; init; }
    public string? AccountantName { get; init; }
    public string? VatRegistrationStatus { get; init; }
    public string? DistrictId { get; init; }
}

public sealed class EdoEmpowermentDto
{
    public string EmpowermentNumber { get; init; } = string.Empty;
    public DateOnly DateOfIssue { get; init; }
    public string AgentName { get; init; } = string.Empty;
    public string AgentPinfl { get; init; } = string.Empty;
}

public sealed class EdoFacturaLineDto
{
    public int Number { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? CatalogCode { get; init; }
    public string? CatalogName { get; init; }
    public string? UnitCode { get; init; }
    public string? UnitName { get; init; }
    public decimal Quantity { get; init; }
    public decimal Amount { get; init; }
    public decimal? TaxRate { get; init; }
    public decimal? TaxAmount { get; init; }
    public bool IsTaxFree { get; init; }
    public IReadOnlyCollection<long> MarkingCodeIds { get; init; } = [];
}

public sealed class EdoDocumentDto
{
    public long Id { get; init; }
    public string? ProviderDocumentId { get; init; }
    public EdoDirection Direction { get; init; }
    public string DocumentType { get; init; } = string.Empty;
    public string? DocumentNumber { get; init; }
    public DateOnly? DocumentDate { get; init; }
    public EdoDocumentStatusDto Status { get; init; } = new();
    public EdoPartyDto? Seller { get; init; }
    public EdoPartyDto? Buyer { get; init; }
    public decimal? TotalAmount { get; init; }
    public string? CurrencyCode { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
    public IReadOnlyCollection<string> MarkingCodes { get; init; } = [];

    // Internal adapter mapping only; it is never serialized by WebAPI.
    [System.Text.Json.Serialization.JsonIgnore]
    public long? LegacyDocumentId { get; init; }
}
