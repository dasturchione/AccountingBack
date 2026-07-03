using Application.Abstractions.Authentication;
using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.WarehouseTransfers;

public class WarehouseTransferByListFilterCriteriaBuilder : ICriteriaBuilder<WarehouseTransferDoc, WarehouseTransferListFilter>
{
    private readonly IUserContext _userContext;

    public WarehouseTransferByListFilterCriteriaBuilder(IUserContext userContext)
    {
        _userContext = userContext;
    }

    public Expression<Func<WarehouseTransferDoc, bool>> Build(WarehouseTransferListFilter options) =>
        x => (!_userContext.OrganizationId.HasValue || x.OrganizationId == _userContext.OrganizationId.Value) &&
             (!options.SourceWarehouseId.HasValue || x.SourceWarehouseId == options.SourceWarehouseId.Value) &&
             (!options.DestinationWarehouseId.HasValue || x.DestinationWarehouseId == options.DestinationWarehouseId.Value) &&
             (!options.StatusId.HasValue || x.StatusId == options.StatusId.Value) &&
             (options.DateFrom == null || x.DocDate >= options.DateFrom) &&
             (options.DateTo == null || x.DocDate <= options.DateTo) &&
             (string.IsNullOrWhiteSpace(options.Search) || x.DocNumber.Contains(options.Search));
}
