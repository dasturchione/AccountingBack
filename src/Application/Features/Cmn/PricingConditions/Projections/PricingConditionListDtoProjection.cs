using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.PricingConditions;

public class PricingConditionListDtoProjection : IProjectionBuilder<PricingCondition, PricingConditionListDto>
{
    public Expression<Func<PricingCondition, PricingConditionListDto>> Build() =>
        x => new PricingConditionListDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            OrganizationName = x.Organization.ShortName,
            PricingMethodId = x.PricingMethodId,
            PricingMethodName = x.PricingMethod.Name,
            PricingMethodCode = x.PricingMethod.Code,
            PricingValue = x.PricingValue,
            RoundingMethodId = x.RoundingMethodId,
            RoundingMethodName = x.RoundingMethod.Name,
            RoundingMethodCode = x.RoundingMethod.Code,
            RoundingPrecision = x.RoundingPrecision,
            StartDate = x.StartDate,
            EndDate = x.EndDate,
            StateId = x.StateId,
            StateName = x.State.FullName,
            CreatedDate = x.CreatedDate
        };
}
