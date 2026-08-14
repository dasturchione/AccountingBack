using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.FaReceipts;

public class FaReceiptListDtoByListFilterCriteriaBuilder : ICriteriaBuilder<FaReceiptListDto, FaReceiptListFilter>
{
    public Expression<Func<FaReceiptListDto, bool>> Build(FaReceiptListFilter options) =>
        x => (!options.CounterpartyId.HasValue || x.CounterpartyId == options.CounterpartyId.Value) &&
             (!options.StatusId.HasValue || x.StatusId == options.StatusId.Value) &&
             (!options.ReceiptTypeId.HasValue || x.ReceiptTypeId == options.ReceiptTypeId.Value) &&
             (!options.DateFrom.HasValue || x.DocDate >= options.DateFrom.Value) &&
             (!options.DateTo.HasValue || x.DocDate <= options.DateTo.Value) &&
             (string.IsNullOrWhiteSpace(options.Search) ||
              x.DocNumber.ToLower().Contains(options.Search.ToLower()) ||
              (x.CounterpartyName != null && x.CounterpartyName.ToLower().Contains(options.Search.ToLower())) ||
              x.CurrencyName.ToLower().Contains(options.Search.ToLower()) ||
              x.StatusName.ToLower().Contains(options.Search.ToLower()));
}
