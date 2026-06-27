using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.SaleConditions;

public class SaleConditionDtoProjection : IProjectionBuilder<SaleCondition, SaleConditionDto>
{
    public Expression<Func<SaleCondition, SaleConditionDto>> Build() =>
        x => new SaleConditionDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            OrganizationName = x.Organization.ShortName,
            CostingMethodId = x.CostingMethodId,
            CostingMethodName = x.CostingMethod.Name,
            CostingMethodCode = x.CostingMethod.Code,
            VatRateId = x.VatRateId,
            VatRateName = x.VatRate.Name,
            VatRateCode = x.VatRate.Code,
            StartDate = x.StartDate,
            EndDate = x.EndDate,
            StateId = x.StateId,
            StateName = x.State.FullName,
            CreatedDate = x.CreatedDate
        };
}
