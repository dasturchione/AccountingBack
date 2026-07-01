using Domain.Entities;
using SharedKernel.Results;

namespace Application.Features.MoneyRegisterBalances;

public interface ISaleMoneyRegisterService
{
    Task<Result<List<MoneyRegisterBalance>>> PostAsync(SaleDoc sale, long postingBatchId, CancellationToken ct = default);
    Task<Result<List<MoneyRegisterBalance>>> ReverseAsync(SaleDoc sale, long postingBatchId, CancellationToken ct = default);
}
