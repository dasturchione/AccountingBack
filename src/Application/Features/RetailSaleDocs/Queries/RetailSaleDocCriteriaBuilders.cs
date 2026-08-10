using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.RetailSaleDocs;

public class RetailSaleDocByListFilterCriteriaBuilder : ICriteriaBuilder<RetailSaleDoc, RetailSaleDocListFilter>
{
    public Expression<Func<RetailSaleDoc, bool>> Build(RetailSaleDocListFilter options) =>
        x => (!options.CounterpartyId.HasValue || x.CounterpartyId == options.CounterpartyId.Value) &&
             (!options.WarehouseId.HasValue || x.WarehouseId == options.WarehouseId.Value) &&
             (!options.CashRegisterId.HasValue || x.CashRegisterId == options.CashRegisterId.Value) &&
             (!options.StatusId.HasValue || x.StatusId == options.StatusId.Value) &&
             (options.DateFrom == null || x.DocDate >= options.DateFrom.Value) &&
             (options.DateTo == null || x.DocDate <= options.DateTo.Value);
}

public class RetailSaleDocListDtoByListFilterCriteriaBuilder : ICriteriaBuilder<RetailSaleDocListDto, RetailSaleDocListFilter>
{
    public Expression<Func<RetailSaleDocListDto, bool>> Build(RetailSaleDocListFilter options) =>
        x => string.IsNullOrWhiteSpace(options.Search) ||
             x.DocNumber.ToLower().Contains(options.Search.ToLower());
}
