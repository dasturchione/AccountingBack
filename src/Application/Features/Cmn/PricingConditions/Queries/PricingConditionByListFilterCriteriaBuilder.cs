using Application.Abstractions.Authentication;
using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.PricingConditions;

public class PricingConditionByListFilterCriteriaBuilder : ICriteriaBuilder<PricingCondition, PricingConditionListFilter>
{
    private readonly IUserContext _userContext;

    public PricingConditionByListFilterCriteriaBuilder(IUserContext userContext)
    {
        _userContext = userContext;
    }

    public Expression<Func<PricingCondition, bool>> Build(PricingConditionListFilter options) =>
        x => _userContext.OrganizationId.HasValue &&
             x.OrganizationId == _userContext.OrganizationId.Value &&
             (!options.PricingMethodId.HasValue || x.PricingMethodId == options.PricingMethodId.Value) &&
             (!options.RoundingMethodId.HasValue || x.RoundingMethodId == options.RoundingMethodId.Value) &&
             (!options.StateId.HasValue || x.StateId == options.StateId.Value);
}
