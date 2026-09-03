using Domain.Entities;
using SharedKernel.Results;

namespace Application.Features.PaymentAcceptancePointOperations;

public interface IPaymentAcceptancePointMoneyRegisterService
{
    Task<Result<List<MoneyRegisterBalance>>> PostAsync(
        PaymentAcceptancePointOperation operation,
        long postingBatchId,
        CancellationToken ct = default);

    Task<Result<List<MoneyRegisterBalance>>> ReverseAsync(
        PaymentAcceptancePointOperation operation,
        long postingBatchId,
        CancellationToken ct = default);

    Task<decimal> GetBalanceAsync(
        int paymentAcceptancePointId,
        short currencyId,
        DateTime asOfDate,
        CancellationToken ct = default);
}
