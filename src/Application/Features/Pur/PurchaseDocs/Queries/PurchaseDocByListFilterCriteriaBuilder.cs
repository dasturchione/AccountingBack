using Application.Abstractions.Authentication;
using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.PurchaseDocs;

public class PurchaseDocByListFilterCriteriaBuilder : ICriteriaBuilder<PurchaseDoc, PurchaseDocListFilter>
{
    private readonly IUserContext _userContext; 
    public PurchaseDocByListFilterCriteriaBuilder(IUserContext userContext)
    {
        _userContext = userContext;
    }

    public Expression<Func<PurchaseDoc, bool>> Build(PurchaseDocListFilter options) =>
        x => (!_userContext.OrganizationId.HasValue || x.OrganizationId == _userContext.OrganizationId.Value) &&
             (!options.CounterpartyId.HasValue || x.CounterpartyId == options.CounterpartyId.Value) &&
             (!options.WarehouseId.HasValue || x.WarehouseId == options.WarehouseId.Value) &&
             (!options.StatusId.HasValue || x.StatusId == options.StatusId.Value) &&
             (options.DateFrom == null || x.DocDate >= options.DateFrom) &&
             (options.DateTo == null || x.DocDate <= options.DateTo);
}
