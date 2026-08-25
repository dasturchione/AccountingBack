using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.CashFiscalTransfers;

public interface ICashFiscalTransferService
{
    Task<Result<PagedResponse<CashFiscalTransferListDto>>> GetAllAsync(CashFiscalTransferListFilter filter, CancellationToken ct = default);
    Task<Result<CashFiscalTransferDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Result<long>> CreateAsync(CashFiscalTransferCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(long id, CashFiscalTransferUpdateDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(long id, CancellationToken ct = default);
    Task<Result> ConfirmAsync(long id, CancellationToken ct = default);
    Task<Result> CancelAsync(long id, CancellationToken ct = default);
}

public interface ICashFiscalTransferLifecycleService
{
    Task<Result> ConfirmAsync(long id, CancellationToken ct = default);
    Task<Result> CancelAsync(long id, CancellationToken ct = default);
}
