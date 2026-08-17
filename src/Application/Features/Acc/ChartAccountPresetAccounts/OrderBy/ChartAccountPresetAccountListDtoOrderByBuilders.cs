using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.ChartAccountPresetAccounts;

public sealed class ChartAccountPresetAccountListDtoOrderByBuilder : IOrderByBuilder<ChartAccountPresetAccount, ChartAccountPresetAccountListDto>
{
    public Func<IQueryable<ChartAccountPresetAccountListDto>, IOrderedQueryable<ChartAccountPresetAccountListDto>> Build() =>
        query => query.OrderBy(x => x.DisplayOrder).ThenBy(x => x.Number).ThenBy(x => x.Id);
}

public sealed class ChartAccountPresetAccountGroupedListDtoOrderByBuilder : IOrderByBuilder<ChartAccountPresetAccount, ChartAccountPresetAccountGroupedListDto>
{
    public Func<IQueryable<ChartAccountPresetAccountGroupedListDto>, IOrderedQueryable<ChartAccountPresetAccountGroupedListDto>> Build() =>
        query => query.OrderBy(x => x.DisplayOrder).ThenBy(x => x.Number).ThenBy(x => x.Id);
}
