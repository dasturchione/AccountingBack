using Domain.Entities;

namespace Application.Features.FaAssets;

public interface IFaAssetCommandRepository
{
    Task CreateAsync(FaAsset entity, CancellationToken ct = default);
    Task UpdateAsync(FaAsset entity, CancellationToken ct = default);
}
