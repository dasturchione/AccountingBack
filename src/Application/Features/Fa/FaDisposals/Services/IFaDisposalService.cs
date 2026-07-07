using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.FaDisposals;

public interface IFaDisposalService
{
    Task<Result<PagedResponse<FaDisposalListDto>>> GetAllAsync(FaDisposalListFilter filter, CancellationToken ct = default);
    Task<Result<FaDisposalDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Result<long>> CreateAsync(FaDisposalCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(long id, FaDisposalUpdateDto dto, CancellationToken ct = default);
    Task<Result> ConfirmAsync(long id, CancellationToken ct = default);
    Task<Result> CancelAsync(long id, CancellationToken ct = default);
}
