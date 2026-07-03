using Domain.Entities;
using SharedKernel.Results;

namespace Application.Features.CounterpartyRegisterBalances;

public interface IPurchaseCounterpartyRegisterService
{
    Task<Result<List<CounterpartyRegisterBalance>>> PostAsync(PurchaseDoc purchase, long postingBatchId, CancellationToken ct = default);
    Task<Result<List<CounterpartyRegisterBalance>>> ReverseAsync(PurchaseDoc purchase, long postingBatchId, CancellationToken ct = default);
}
