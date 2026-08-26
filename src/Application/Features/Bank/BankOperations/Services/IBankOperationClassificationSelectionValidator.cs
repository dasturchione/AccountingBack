using SharedKernel.Results;

namespace Application.Features.BankOperations;

public interface IBankOperationClassificationSelectionValidator
{
    Task<Result> ValidateAsync(
        int organizationId,
        int bankAccountId,
        short? categoryId,
        int? ruleId,
        CancellationToken ct = default);
}
