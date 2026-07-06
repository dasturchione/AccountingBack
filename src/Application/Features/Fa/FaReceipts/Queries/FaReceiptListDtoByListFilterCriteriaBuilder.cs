using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.FaReceipts;

public class FaReceiptListDtoByListFilterCriteriaBuilder : ICriteriaBuilder<FaReceiptListDto, FaReceiptListFilter>
{
    public Expression<Func<FaReceiptListDto, bool>> Build(FaReceiptListFilter options) =>
        x => (!options.CounterpartyId.HasValue || x.CounterpartyId == options.CounterpartyId.Value) &&
             (!options.WarehouseId.HasValue || x.WarehouseId == options.WarehouseId.Value) &&
             (!options.StatusId.HasValue || x.StatusId == options.StatusId.Value) &&
             (string.IsNullOrWhiteSpace(options.ReceiptType) || x.ReceiptType == options.ReceiptType) &&
             (!options.DateFrom.HasValue || x.DocDate >= options.DateFrom.Value) &&
             (!options.DateTo.HasValue || x.DocDate <= options.DateTo.Value) &&
             (string.IsNullOrWhiteSpace(options.Search) ||
              x.DocNumber.ToLower().Contains(options.Search.ToLower()) ||
              (x.CounterpartyName != null && x.CounterpartyName.ToLower().Contains(options.Search.ToLower())) ||
              (x.WarehouseName != null && x.WarehouseName.ToLower().Contains(options.Search.ToLower())) ||
              x.CurrencyName.ToLower().Contains(options.Search.ToLower()) ||
              x.StatusName.ToLower().Contains(options.Search.ToLower()));
}
