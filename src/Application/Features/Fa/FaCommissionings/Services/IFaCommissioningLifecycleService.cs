using SharedKernel.Results;

namespace Application.Features.FaCommissionings;

public interface IFaCommissioningLifecycleService
{
    Task<Result> ConfirmAsync(long id, CancellationToken ct = default);
    Task<Result> CancelAsync(long id, CancellationToken ct = default);
}
