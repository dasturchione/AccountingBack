using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.SaleConditions;

public class SaleConditionListDtoProjection : IProjectionBuilder<SaleCondition, SaleConditionListDto>
{
    public Expression<Func<SaleCondition, SaleConditionListDto>> Build() =>
        x => new SaleConditionListDto
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
