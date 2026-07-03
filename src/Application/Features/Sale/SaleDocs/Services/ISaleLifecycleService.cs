using SharedKernel.Results;

namespace Application.Features.SaleDocs;

public interface ISaleLifecycleService
{
    Task<Result> ConfirmAsync(long id, SaleDocConfirmDto dto, CancellationToken ct = default);
    Task<Result> CancelAsync(long id, CancellationToken ct = default);
}
