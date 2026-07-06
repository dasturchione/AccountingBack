using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.FaAssets;

public interface IFaAssetService
{
    Task<Result<PagedResponse<FaAssetListDto>>> GetAllAsync(FaAssetListFilter filter, CancellationToken ct = default);
    Task<Result<FaAssetDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Result<long>> CreateAsync(FaAssetCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(long id, FaAssetUpdateDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(long id, CancellationToken ct = default);
}
