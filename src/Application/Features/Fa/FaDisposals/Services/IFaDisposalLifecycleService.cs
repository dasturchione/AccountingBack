using SharedKernel.Results;

namespace Application.Features.FaDisposals;

public interface IFaDisposalLifecycleService
{
    Task<Result> ConfirmAsync(long id, CancellationToken ct = default);
    Task<Result> CancelAsync(long id, CancellationToken ct = default);
}
