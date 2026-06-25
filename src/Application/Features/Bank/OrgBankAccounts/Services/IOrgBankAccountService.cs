using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.OrgBankAccounts;

public interface IOrgBankAccountService
{
    Task<Result<PagedResponse<OrgBankAccountListDto>>> GetAllAsync(OrgBankAccountListFilter filter, CancellationToken ct = default);
    Task<Result<OrgBankAccountDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<OrgBankAccountCreateResultDto>> CreateAsync(OrgBankAccountCreateDto dto, CancellationToken ct = default);
    Task<Result<List<OrgBankAccountCreateResultDto>>> CreateManyAsync(OrgBankAccountCreateManyDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(int id, OrgBankAccountUpdateDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(int id, CancellationToken ct = default);
}
