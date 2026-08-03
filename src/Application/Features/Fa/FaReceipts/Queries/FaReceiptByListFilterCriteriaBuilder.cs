using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.FaReceipts;

public class FaReceiptByListFilterCriteriaBuilder : ICriteriaBuilder<FaReceiptDoc, FaReceiptListFilter>
{
    public Expression<Func<FaReceiptDoc, bool>> Build(FaReceiptListFilter options) =>
        x => (!options.CounterpartyId.HasValue || x.CounterpartyId == options.CounterpartyId.Value) &&
             (!options.WarehouseId.HasValue || x.WarehouseId == options.WarehouseId.Value) &&
             (!options.StatusId.HasValue || x.StatusId == options.StatusId.Value) &&
             (!options.ReceiptTypeId.HasValue || x.ReceiptTypeId == options.ReceiptTypeId.Value) &&
             (!options.DateFrom.HasValue || x.DocDate >= options.DateFrom.Value) &&
             (!options.DateTo.HasValue || x.DocDate <= options.DateTo.Value);
}
