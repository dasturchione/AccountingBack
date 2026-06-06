using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.OrgBankAccounts;

public interface IOrgBankAccountService
{
    Task<Result<PagedResponse<OrgBankAccountListDto>>> GetAllAsync(OrgBankAccountListFilter filter, CancellationToken ct = default);
    Task<Result<OrgBankAccountDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<int>> CreateAsync(OrgBankAccountCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(int id, OrgBankAccountUpdateDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(int id, CancellationToken ct = default);
}
