using SharedKernel.Results;

namespace Application.Features.FaRevaluations;

public interface IFaRevaluationLifecycleService
{
    Task<Result> ConfirmAsync(long id, CancellationToken ct = default);
    Task<Result> CancelAsync(long id, CancellationToken ct = default);
}
