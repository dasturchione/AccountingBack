namespace Application.Features.Contracts;

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
