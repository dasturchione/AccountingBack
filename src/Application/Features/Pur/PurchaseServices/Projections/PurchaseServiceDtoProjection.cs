using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.PurchaseServices;

public class PurchaseServiceDtoProjection : IProjectionBuilder<PurchaseService, PurchaseServiceDto>
{
    public Expression<Func<PurchaseService, PurchaseServiceDto>> Build() =>
        x => new PurchaseServiceDto
        {
            Id              = x.Id,
            Name            = x.Name,
            Description     = x.Description,
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
