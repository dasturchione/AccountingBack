using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.FaAssets;

public class FaAssetListDtoByListFilterCriteriaBuilder : ICriteriaBuilder<FaAssetListDto, FaAssetListFilter>
{
    public Expression<Func<FaAssetListDto, bool>> Build(FaAssetListFilter options) =>
        x => (!options.FaGroupId.HasValue || x.FaGroupId == options.FaGroupId.Value) &&
             (!options.StatusId.HasValue || x.StatusId == options.StatusId.Value) &&
             (string.IsNullOrEmpty(options.Search) ||
              x.InventoryNumber.ToLower().Contains(options.Search.ToLower()) ||
              x.Name.ToLower().Contains(options.Search.ToLower()) ||
              x.FaGroupName.ToLower().Contains(options.Search.ToLower()) ||
              x.StatusName.ToLower().Contains(options.Search.ToLower()));
}
