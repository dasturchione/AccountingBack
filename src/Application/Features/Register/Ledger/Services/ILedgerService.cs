using SharedKernel.Results;

namespace Application.Features.Ledger;

public interface ILedgerService
{
    Task<Result<LedgerDto>> GetAsync(LedgerFilter filter, CancellationToken ct = default);
}
