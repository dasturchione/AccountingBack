using Domain.Entities;
using SharedKernel.Results;

namespace Application.Features.CounterpartyRegisterBalances;

public interface ISaleCounterpartyRegisterService
{
    Task<Result<List<CounterpartyRegisterBalance>>> PostAsync(SaleDoc sale, long postingBatchId, CancellationToken ct = default);
    Task<Result<List<CounterpartyRegisterBalance>>> ReverseAsync(SaleDoc sale, long postingBatchId, CancellationToken ct = default);
}
