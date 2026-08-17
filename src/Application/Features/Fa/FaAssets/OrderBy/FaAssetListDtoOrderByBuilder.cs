using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.FaAssets;

public sealed class FaAssetListDtoOrderByBuilder : IOrderByBuilder<FaAsset, FaAssetListDto>
{
    public Func<IQueryable<FaAssetListDto>, IOrderedQueryable<FaAssetListDto>> Build() =>
        query => query.OrderBy(x => x.InventoryNumber).ThenBy(x => x.Id);
}
