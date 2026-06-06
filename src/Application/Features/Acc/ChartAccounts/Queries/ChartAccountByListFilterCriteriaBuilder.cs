using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.ChartAccounts;

public class ChartAccountByListFilterCriteriaBuilder : ICriteriaBuilder<ChartAccount, ChartAccountListFilter>
{
    public Expression<Func<ChartAccount, bool>> Build(ChartAccountListFilter options) =>
        x => (!options.OrganizationId.HasValue || x.OrganizationId == options.OrganizationId.Value) &&
             (!options.ParentId.HasValue || x.ParentId == options.ParentId.Value) &&
             (!options.IsGroup.HasValue || x.IsGroup == options.IsGroup.Value);
}
