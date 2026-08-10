using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.FiscalCashRegisters;

public interface IFiscalCashRegisterService
{
    Task<Result<PagedResponse<FiscalCashRegisterListDto>>> GetAllAsync(FiscalCashRegisterListFilter filter, CancellationToken ct = default);
    Task<Result<FiscalCashRegisterDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<int>> CreateAsync(FiscalCashRegisterCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(int id, FiscalCashRegisterUpdateDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(int id, CancellationToken ct = default);
}
