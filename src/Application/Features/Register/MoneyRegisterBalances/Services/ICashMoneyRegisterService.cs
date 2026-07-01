using Domain.Entities;
using SharedKernel.Results;

namespace Application.Features.MoneyRegisterBalances;

public interface ICashMoneyRegisterService
{
    Task<Result<List<MoneyRegisterBalance>>> PostAsync(CashOperation cashOperation, long postingBatchId, CancellationToken ct = default);
    Task<Result<List<MoneyRegisterBalance>>> ReverseAsync(CashOperation cashOperation, long postingBatchId, CancellationToken ct = default);
    Task<decimal> GetCashBoxBalanceAsync(int cashBoxId, DateTime asOfDate, CancellationToken ct = default);
}
