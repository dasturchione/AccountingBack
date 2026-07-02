using Domain.Entities;
using SharedKernel.Results;

namespace Application.Features.MoneyRegisterBalances;

public interface IBankMoneyRegisterService
{
    Task<Result<List<MoneyRegisterBalance>>> PostAsync(BankOperation bankOperation, long postingBatchId, CancellationToken ct = default);
    Task<Result<List<MoneyRegisterBalance>>> ReverseAsync(BankOperation bankOperation, long postingBatchId, CancellationToken ct = default);
    Task<decimal> GetBankAccountBalanceAsync(int bankAccountId, DateTime asOfDate, CancellationToken ct = default);
}
