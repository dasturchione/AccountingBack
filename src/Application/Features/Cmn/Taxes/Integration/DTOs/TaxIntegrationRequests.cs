namespace Application.Features.Cmn.Taxes.Integration.DTOs;

public sealed class TaxLookupRequestDto
{
    public string ProviderCode { get; init; } = null!;
    public string? Query { get; init; }
    public int? OrganizationId { get; init; }
    public DateOnly? EffectiveDate { get; init; }
}

public sealed class TaxDocumentRequestDto
{
    public string ProviderCode { get; init; } = null!;
    public int? OrganizationId { get; init; }
    public string? DocumentNumber { get; init; }
    public string? Payload { get; init; }
    public string? ExternalDocumentId { get; init; }

    // Didox user-key (company token) obtained via the frontend E-IMZO auth flow. Request-scoped.
    public string? CompanyToken { get; init; }
}

public sealed class DidoxAuthSignatureRequestDto
{
    public string TaxId { get; init; } = null!;
    public string Signature { get; init; } = null!;
    public string? Locale { get; init; }
}

public sealed class DidoxAuthPasswordRequestDto
{
    public string TaxId { get; init; } = null!;
    public string Password { get; init; } = null!;
    public string? Locale { get; init; }
}

public sealed class DidoxSignRequestDto
{
    public string DocumentId { get; init; } = null!;

    // PKCS#7 timestamp signature (base64) produced by the frontend E-IMZO flow.
    public string Signature { get; init; } = null!;

    // Didox user-key (company token). Request-scoped.
    public string? CompanyToken { get; init; }
}
