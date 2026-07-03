using Domain.Entities;
using SharedKernel.Results;

namespace Application.Features.CounterpartyRegisterBalances;

public interface ICashCounterpartyRegisterService
{
    Task<Result<List<CounterpartyRegisterBalance>>> PostAsync(CashOperation cashOperation, long postingBatchId, CancellationToken ct = default);
    Task<Result<List<CounterpartyRegisterBalance>>> ReverseAsync(CashOperation cashOperation, long postingBatchId, CancellationToken ct = default);
}
