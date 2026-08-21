namespace Application.Features.Contracts;

public sealed record ProviderContractReconciliationCreateDto
{
    public bool Confirm { get; init; }
    public int CounterpartyId { get; init; }
    public string ProviderCode { get; init; } = string.Empty;
    public string ProviderContractNumber { get; init; } = string.Empty;
    public DateOnly ProviderContractDate { get; init; }
    public short ContractTypeId { get; init; }
    public DateTime ContractDate { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public string? Comment { get; init; }
}

public sealed class ProviderContractReconciliationResultDto
{
    public int OrganizationId { get; init; }
    public long ContractId { get; init; }
    public string Status { get; init; } = string.Empty;
    public string IdempotencyKey { get; init; } = string.Empty;
    public string ProviderCode { get; init; } = string.Empty;
    public string ProviderContractNumber { get; init; } = string.Empty;
    public DateOnly ProviderContractDate { get; init; }
}
