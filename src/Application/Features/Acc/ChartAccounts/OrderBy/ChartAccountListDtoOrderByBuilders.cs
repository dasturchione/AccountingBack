using Application.Features.ChartAccounts;
using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.Acc.ChartAccounts;

public sealed class ChartAccountListDtoOrderByBuilder : IOrderByBuilder<ChartAccount, ChartAccountListDto>
{
    public Func<IQueryable<ChartAccountListDto>, IOrderedQueryable<ChartAccountListDto>> Build() =>
        query => query.OrderBy(x => x.Number).ThenBy(x => x.Id);
}

public sealed class ChartAccountGroupedListDtoOrderByBuilder : IOrderByBuilder<ChartAccount, ChartAccountGroupedListDto>
{
    public Func<IQueryable<ChartAccountGroupedListDto>, IOrderedQueryable<ChartAccountGroupedListDto>> Build() =>
        query => query.OrderBy(x => x.Number).ThenBy(x => x.Id);
}
