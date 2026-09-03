using SharedKernel.Results;

namespace Application.Features.AccountingRegisterEntries;

public interface IAccountingRegisterEntryRebuildService
{
    Task<Result<AccountingRegisterEntryRebuildDto>> RebuildAsync(
        short documentTypeId,
        long documentId,
        CancellationToken ct = default);
}
