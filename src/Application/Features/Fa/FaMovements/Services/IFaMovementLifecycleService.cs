using SharedKernel.Results;

namespace Application.Features.FaMovements;

public interface IFaMovementLifecycleService
{
    Task<Result> ConfirmAsync(long id, CancellationToken ct = default);
    Task<Result> CancelAsync(long id, CancellationToken ct = default);
}
