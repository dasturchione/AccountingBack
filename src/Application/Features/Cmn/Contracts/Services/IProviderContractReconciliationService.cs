using SharedKernel.Results;

namespace Application.Features.Contracts;

public interface IProviderContractReconciliationService
{
    Task<Result<ProviderContractReconciliationResultDto>> ReconcileAsync(
        ProviderContractReconciliationCreateDto dto,
        CancellationToken ct = default);
}
