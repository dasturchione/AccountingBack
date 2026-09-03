using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.Banks;

public interface IBankService
{
    Task<Result<PagedResponse<BankListDto>>> GetAllAsync(BankListFilter filter, CancellationToken ct = default);
    Task<Result<BankDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<List<BankBranchDto>>> GetBranchesAsync(int bankId, CancellationToken ct = default);
    Task<Result<BankBranchDto>> GetBranchByMfoAsync(string mfo, CancellationToken ct = default);
}
