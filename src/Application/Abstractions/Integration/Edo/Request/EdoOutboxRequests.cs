namespace Application.Abstractions.Integration.Edo;

public sealed class EdoOutboxFacturaCreateRequestDto
{
    public long InternalDocumentId { get; init; }
    public string InternalDocumentType { get; init; } = string.Empty;
    public EdoPartyDto Seller { get; init; } = new();
    public EdoPartyDto Buyer { get; init; } = new();
    public string DocumentNumber { get; init; } = string.Empty;
    public DateOnly DocumentDate { get; init; }
    public string? ContractNumber { get; init; }
    public DateOnly? ContractDate { get; init; }
    public EdoEmpowermentDto? Empowerment { get; init; }
    public IReadOnlyCollection<EdoFacturaLineDto> Lines { get; init; } = [];
    public string IdempotencyKey { get; init; } = string.Empty;
}

public sealed class EdoOutboxSignRequestDto
{
    public string IdempotencyKey { get; init; } = string.Empty;
    public string? CertificateSerialNumber { get; init; }
    public string? SigningSessionId { get; init; }
    public EdoSigningMode? SigningMode { get; init; }
    public string? PreparedPkcs7 { get; init; }
    public string? SignatureHex { get; init; }
    public string? Hash { get; init; }
}
