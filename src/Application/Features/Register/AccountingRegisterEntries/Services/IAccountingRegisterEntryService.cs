using Application.Common.Pagination;
using Application.Features.Register.AccountingRegisterEntries;
using SharedKernel.Results;

namespace Application.Features.AccountingRegisterEntries;

public interface IAccountingRegisterEntryService
{
    Task<Result<PagedResponse<AccountingRegisterEntryListDto>>> GetAllAsync(AccountingRegisterEntryListFilter filter, CancellationToken ct = default);
    Task<Result<AccountingRegisterEntryDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Result<long>> CreateAsync(AccountingRegisterEntryCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(long id, AccountingRegisterEntryUpdateDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(long id, CancellationToken ct = default);
    Task<Result<List<AccountingPostingDto>>> GetPostingAsync(short documentTypeId, long documentId, CancellationToken ct = default);
    Task<Result<List<AccountingPostingDto>>> GetDailyPostingAsync(DateTime startDate, DateTime endDate, short? documentTypeId, CancellationToken ct = default);
}