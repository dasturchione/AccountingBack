using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.Contracts;

public interface IContractService
{
    Task<Result<PagedResponse<ContractListDto>>> GetAllAsync(ContractListFilter filter, CancellationToken ct = default);
    Task<Result<ContractDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Result<long>> CreateAsync(ContractCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(long id, ContractUpdateDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(long id, CancellationToken ct = default);
}
