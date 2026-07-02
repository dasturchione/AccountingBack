using Domain.Entities;
using SharedKernel.Results;

namespace Application.Features.CounterpartyRegisterBalances;

public interface IBankCounterpartyRegisterService
{
    Task<Result<List<CounterpartyRegisterBalance>>> PostAsync(BankOperation bankOperation, long postingBatchId, CancellationToken ct = default);
    Task<Result<List<CounterpartyRegisterBalance>>> ReverseAsync(BankOperation bankOperation, long postingBatchId, CancellationToken ct = default);
}
