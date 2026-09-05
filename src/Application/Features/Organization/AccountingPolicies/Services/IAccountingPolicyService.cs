using Application.Features.AccountingPolicies.DTOs;
using SharedKernel.Results;

namespace Application.Features.AccountingPolicies.Services;

public interface IAccountingPolicyService
{
    Task<Result<AccountingPolicyCurrentDto>> GetCurrentAsync(
        DateOnly? effectiveOn,
        CancellationToken cancellationToken = default);

    Task<Result<AccountingPolicyHistoryDto>> GetHistoryAsync(
        DateOnly? dateFrom,
        DateOnly? dateTo,
        CancellationToken cancellationToken = default);

    Task<Result<AccountingPolicyImpactDto>> GetImpactAsync(
        DateOnly effectiveOn,
        string? documentType,
        CancellationToken cancellationToken = default);

    Task<Result<AccountingPolicyCurrentDto>> UpdateAsync(
        AccountingPolicyUpdateDto request,
        CancellationToken cancellationToken = default);
}
