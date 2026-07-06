using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.FaAssets;

public class FaAssetByListFilterCriteriaBuilder : ICriteriaBuilder<FaAsset, FaAssetListFilter>
{
    public Expression<Func<FaAsset, bool>> Build(FaAssetListFilter options) =>
        x => (!options.FaGroupId.HasValue || x.FaGroupId == options.FaGroupId.Value) &&
             (!options.StatusId.HasValue || x.StatusId == options.StatusId.Value);
}
