using System.Text.Json;

namespace Application.Abstractions.Integration.Edo;

[System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
public enum EdoDirection
{
    INBOX,
    OUTBOX
}

[System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
public enum EdoDocumentCategory
{
    INBOX,
    OUTBOX,
    DRAFTS,
    REJECTED,
    DELETED_ARCHIVED,
    ALL
}

[System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
public enum EdoDocumentStatusCode
{
    UNKNOWN,
    PENDING_SIGNATURE,
    PARTNER_SIGNATURE_PENDING,
    AGENT_SIGNATURE_PENDING,
    DRAFT,
    PENDING,
    SIGNED,
    SENT,
    RECEIVED,
    REJECTED,
    DELETED,
    ARCHIVED,
    COMPLETED,
    CANCELLED,
    FAILED,
    RECONCILIATION_REQUIRED
}

public sealed class EdoDocumentStatusDto
{
    public EdoDocumentStatusCode Code { get; init; }
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public EdoDocumentStatusCode? LocalCode { get; init; }
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? ProviderStatusCode { get; init; }
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? ProviderRawStatus { get; init; }
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; init; }
    public bool IsTerminal { get; init; }
    public bool IsSuccessful { get; init; }
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public DateTimeOffset? CheckedAt { get; init; }
    public bool IsReconciliationRequired { get; init; }
}

public sealed class EdoProviderDocumentStatusResponseDto
{
    public string DocumentIdentity { get; init; } = string.Empty;
    public string ProviderDocumentId { get; init; } = string.Empty;
    public EdoProviderCode ProviderCode { get; init; }
    public EdoDirection Direction { get; init; } = EdoDirection.OUTBOX;
    public EdoDocumentStatusDto Status { get; init; } = new();
}

public sealed class EdoPartyDto
{
    public string Name { get; init; } = string.Empty;
    public string TaxIdentifier { get; init; } = string.Empty;
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? BankCode { get; init; }
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? AccountNumber { get; init; }
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? Address { get; init; }
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? BranchCode { get; init; }
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? BranchName { get; init; }
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? DirectorName { get; init; }
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? AccountantName { get; init; }
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? VatRegistrationStatus { get; init; }
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
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
    public long? Id { get; init; }
    public bool StatusCheckable { get; init; }
    public EdoProviderCode ProviderCode { get; init; }
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? DocumentIdentity => ProviderDocumentId;
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? ProviderDocumentId { get; init; }
    public EdoDirection Direction { get; init; }
    public EdoDocumentCategory Category { get; init; }
    public string DocumentType { get; init; } = string.Empty;
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? DocumentNumber { get; init; }
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public DateOnly? DocumentDate { get; init; }
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? DocumentDateTime { get; init; }
    public EdoDocumentStatusDto Status { get; init; } = new();
    [System.Text.Json.Serialization.JsonIgnore]
    public EdoEmpowermentDto? Empowerment { get; init; }
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public EdoPartyDto? Seller { get; init; }
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public EdoPartyDto? Buyer { get; init; }
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public decimal? TotalAmount { get; init; }
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? CurrencyCode { get; init; }
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public DateTimeOffset? CreatedAt { get; init; }
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public DateTimeOffset? UpdatedAt { get; init; }
    [System.Text.Json.Serialization.JsonIgnore]
    public IReadOnlyCollection<string> MarkingCodes { get; init; } = [];
    public bool HasMarking => MarkingCodes.Count > 0;
    public int MarkingCount => MarkingCodes.Count;
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? VerificationState { get; init; }
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? SourceType { get; init; }
    [System.Text.Json.Serialization.JsonIgnore]
    public IReadOnlyDictionary<string, JsonElement> ProviderFields { get; init; } =
        new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);

    // Normalized read-only data used by the Purchase preview use-case.
    // These fields are intentionally not part of the public EDO response contract.
    [System.Text.Json.Serialization.JsonIgnore]
    public string? PreviewSellerTin { get; init; }
    [System.Text.Json.Serialization.JsonIgnore]
    public string? PreviewContractNumber { get; init; }
    [System.Text.Json.Serialization.JsonIgnore]
    public DateOnly? PreviewContractDate { get; init; }
    [System.Text.Json.Serialization.JsonIgnore]
    public IReadOnlyCollection<EdoDocumentPreviewLineDto> PreviewLines { get; init; } = [];

    // Internal adapter mapping only; it is never serialized by WebAPI.
    [System.Text.Json.Serialization.JsonIgnore]
    public long? LegacyDocumentId { get; init; }
}

public sealed class EdoDocumentPreviewLineDto
{
    public int Number { get; init; }
    public string? CatalogCode { get; init; }
    [System.Text.Json.Serialization.JsonIgnore]
    public string? CatalogName { get; init; }
    public string? PackageCode { get; init; }
    public string? PackageName { get; init; }
    [System.Text.Json.Serialization.JsonIgnore]
    public bool IsService { get; init; }
    [System.Text.Json.Serialization.JsonIgnore]
    public decimal? NetAmount { get; init; }
    [System.Text.Json.Serialization.JsonIgnore]
    public decimal? VatAmount { get; init; }
    public decimal? Quantity { get; init; }
    public decimal? UnitPrice { get; init; }
    public decimal? VatRate { get; init; }
    public decimal? TotalWithVat { get; init; }
    [System.Text.Json.Serialization.JsonIgnore]
    public IReadOnlyCollection<string> MarkingCodes { get; init; } = [];
}
