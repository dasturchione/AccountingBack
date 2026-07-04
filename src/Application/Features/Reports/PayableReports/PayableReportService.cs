using Application.Common.Pagination;
using Application.Features.CounterpartyRegisterBalances;
using SharedKernel.Results;

namespace Application.Features.Reports.PayableReports;

public interface IPayableReportService
{
    Task<Result<PagedResponse<CounterpartyRegisterBalanceListDto>>> GetAllAsync(CounterpartyRegisterBalanceListFilter filter, CancellationToken ct = default);
    Task<Result<CounterpartyRegisterBalanceDto>> GetByIdAsync(long id, CancellationToken ct = default);
}

public sealed class PayableReportService : IPayableReportService
{
    private readonly ICounterpartyRegisterBalanceService _counterpartyRegisterBalanceService;

    public PayableReportService(ICounterpartyRegisterBalanceService counterpartyRegisterBalanceService)
    {
        _counterpartyRegisterBalanceService = counterpartyRegisterBalanceService;
    }

    public Task<Result<PagedResponse<CounterpartyRegisterBalanceListDto>>> GetAllAsync(CounterpartyRegisterBalanceListFilter filter, CancellationToken ct = default) =>
        _counterpartyRegisterBalanceService.GetAllAsync(filter, ct);

    public Task<Result<CounterpartyRegisterBalanceDto>> GetByIdAsync(long id, CancellationToken ct = default) =>
        _counterpartyRegisterBalanceService.GetByIdAsync(id, ct);
}
