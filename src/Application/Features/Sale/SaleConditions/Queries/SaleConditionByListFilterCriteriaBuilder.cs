using Application.Abstractions.Authentication;
using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.SaleConditions;

public class SaleConditionByListFilterCriteriaBuilder : ICriteriaBuilder<SaleCondition, SaleConditionListFilter>
{
    private readonly IUserContext _userContext;

    public SaleConditionByListFilterCriteriaBuilder(IUserContext userContext)
    {
        _userContext = userContext;
    }

    public Expression<Func<SaleCondition, bool>> Build(SaleConditionListFilter options) =>
        x => _userContext.OrganizationId.HasValue &&
             x.OrganizationId == _userContext.OrganizationId.Value &&
             (!options.CostingMethodId.HasValue || x.CostingMethodId == options.CostingMethodId.Value) &&
             (!options.VatRateId.HasValue || x.VatRateId == options.VatRateId.Value) &&
             (!options.StateId.HasValue || x.StateId == options.StateId.Value);
}
