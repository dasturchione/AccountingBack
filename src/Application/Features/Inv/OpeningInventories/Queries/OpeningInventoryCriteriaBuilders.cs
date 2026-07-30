using Application.Abstractions.Authentication;
using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Inv.OpeningInventories;

public sealed class OpeningInventoryByListFilterCriteriaBuilder
    : ICriteriaBuilder<OpeningInventory, OpeningInventoryListFilter>
{
    private readonly IUserContext _userContext;

    public OpeningInventoryByListFilterCriteriaBuilder(IUserContext userContext)
    {
        _userContext = userContext;
    }

    public Expression<Func<OpeningInventory, bool>> Build(OpeningInventoryListFilter options) =>
        x => (!_userContext.OrganizationId.HasValue || x.OrganizationId == _userContext.OrganizationId.Value) &&
             (!options.CounterpartyId.HasValue || x.CounterpartyId == options.CounterpartyId.Value) &&
             (!options.WarehouseId.HasValue || x.WarehouseId == options.WarehouseId.Value) &&
             (!options.StatusId.HasValue || x.StatusId == options.StatusId.Value) &&
             (!options.DateFrom.HasValue || x.DocDate >= options.DateFrom.Value) &&
             (!options.DateTo.HasValue || x.DocDate <= options.DateTo.Value);
}

public sealed class OpeningInventoryListDtoByListFilterCriteriaBuilder
    : ICriteriaBuilder<OpeningInventoryListDto, OpeningInventoryListFilter>
{
    public Expression<Func<OpeningInventoryListDto, bool>> Build(OpeningInventoryListFilter options) =>
        x => string.IsNullOrWhiteSpace(options.Search) ||
             x.DocNumber.ToLower().Contains(options.Search.ToLower()) ||
             x.CounterpartyName.ToLower().Contains(options.Search.ToLower()) ||
             x.WarehouseName.ToLower().Contains(options.Search.ToLower());
}
