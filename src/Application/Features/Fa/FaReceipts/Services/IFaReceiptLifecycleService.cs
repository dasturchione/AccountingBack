using SharedKernel.Results;

namespace Application.Features.FaReceipts;

public interface IFaReceiptLifecycleService
{
    Task<Result> ConfirmAsync(long id, CancellationToken ct = default);
    Task<Result> CancelAsync(long id, CancellationToken ct = default);
}
