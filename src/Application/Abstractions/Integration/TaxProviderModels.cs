namespace Application.Abstractions.Integration;

public sealed class TaxProviderLookupRequestDto
{
    public string ProviderCode { get; init; } = null!;
    public string? Query { get; init; }
    public int? OrganizationId { get; init; }
    public DateOnly? EffectiveDate { get; init; }
}

public sealed class TaxProviderLookupItemDto
{
    public string Code { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public bool IsActive { get; init; }
    public IDictionary<string, string?> Metadata { get; init; } = new Dictionary<string, string?>();
}

public sealed class TaxProviderOperationRequestDto
{
    public string ProviderCode { get; init; } = null!;
    public int? OrganizationId { get; init; }
    public string? DocumentNumber { get; init; }
    public string? Payload { get; init; }
    public string? ExternalDocumentId { get; init; }

}

public sealed class TaxProviderOperationResultDto
{
    public string ProviderCode { get; init; } = null!;
    public string Operation { get; init; } = null!;
    public string? ExternalDocumentId { get; init; }
    public string? StatusCode { get; init; }
    public string? StatusName { get; init; }
    public bool IsSuccessful { get; init; }
    public string? Message { get; init; }
    public DateTime RequestedAt { get; init; }
}

public sealed class TaxProviderInfoDto
{
    public string Code { get; init; } = null!;
    public string Name { get; init; } = null!;
    public bool SupportsStatus { get; init; } = true;
}

public sealed class TaxProviderStatusDto
{
    public string ProviderCode { get; init; } = null!;
    public string ProviderName { get; init; } = null!;
    public bool IsEnabled { get; init; }
    public bool IsConfigured { get; init; }
    public string? Endpoint { get; init; }
    public string? LastError { get; init; }
    public DateTime? CheckedAt { get; init; }
}

public interface ITaxProvider
{
    string Code { get; }
    string Name { get; }
    Task<TaxProviderStatusDto> GetStatusAsync(CancellationToken ct = default);
}

public interface ITaxLookupProvider : ITaxProvider
{
    Task<IReadOnlyCollection<TaxProviderLookupItemDto>> SearchAsync(TaxProviderLookupRequestDto request, CancellationToken ct = default);
    Task<TaxProviderLookupItemDto?> GetByCodeAsync(string code, CancellationToken ct = default);
}

public interface ITaxDocumentProvider : ITaxProvider
{
    Task<TaxProviderOperationResultDto> SubmitAsync(TaxProviderOperationRequestDto request, CancellationToken ct = default);
    Task<TaxProviderOperationResultDto> GetDocumentStatusAsync(TaxProviderOperationRequestDto request, CancellationToken ct = default);
    Task<TaxProviderOperationResultDto> CancelAsync(TaxProviderOperationRequestDto request, CancellationToken ct = default);
}

public interface ITaxProviderFactory
{
    ITaxProvider? Resolve(string? providerCode);
    IReadOnlyCollection<TaxProviderInfoDto> GetSupportedProviders();
    IReadOnlyCollection<TaxProviderInfoDto> GetSupportedLookupProviders();
    IReadOnlyCollection<TaxProviderInfoDto> GetSupportedDocumentProviders();
}
