using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.PurchaseServices;

public class PurchaseServiceListDtoProjection : IProjectionBuilder<PurchaseService, PurchaseServiceListDto>
{
    public Expression<Func<PurchaseService, PurchaseServiceListDto>> Build() =>
        x => new PurchaseServiceListDto
        {
            Id              = x.Id,
            Name            = x.Name,
            ServiceTypeId   = x.ServiceTypeId,
            ServiceTypeName = x.ServiceType.Name,
            AccountId       = x.ServiceType.AccountId,
            AccountName     = x.ServiceType.Account.Name,
            VatApplicable   = x.ServiceType.VatApplicable,
            StateId         = x.StateId,
            StateName       = x.State.FullName,
            CreatedDate     = x.CreatedDate
        };
}
