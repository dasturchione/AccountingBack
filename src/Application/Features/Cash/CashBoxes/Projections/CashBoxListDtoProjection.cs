using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.CashBoxes;

public class CashBoxListDtoProjection : IProjectionBuilder<CashBox, CashBoxListDto>
{
    public Expression<Func<CashBox, CashBoxListDto>> Build() =>
        x => new CashBoxListDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            OrganizationName = x.Organization.ShortName,
            BranchId = x.BranchId,
            BranchName = x.Branch != null ? x.Branch.Name : null,
            Code = x.Code,
            Name = x.Name,
            CurrencyId = x.CurrencyId,
            CurrencyName = x.Currency.Name,
            StateId = x.StateId,
            StateName = x.State.FullName,
            CreatedDate = x.CreatedDate
        };
}
