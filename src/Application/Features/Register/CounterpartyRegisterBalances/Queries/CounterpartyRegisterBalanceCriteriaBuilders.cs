using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.CounterpartyRegisterBalances;


public class CounterpartyRegisterBalanceByListFilterCriteriaBuilder : ICriteriaBuilder<CounterpartyRegisterBalance, CounterpartyRegisterBalanceListFilter>
{
    public Expression<Func<CounterpartyRegisterBalance, bool>> Build(CounterpartyRegisterBalanceListFilter options) =>
        x => (!options.DocumentTypeId.HasValue || x.DocumentTypeId == options.DocumentTypeId.Value) &&
             (!options.DocumentId.HasValue || x.DocumentId == options.DocumentId.Value) &&
             (!options.CounterpartyId.HasValue || x.CounterpartyId == options.CounterpartyId.Value) &&
             (!options.OperationTypeId.HasValue || x.OperationTypeId == options.OperationTypeId.Value) &&
             (!options.CurrencyId.HasValue || x.CurrencyId == options.CurrencyId.Value) &&
             (options.DateFrom == null || x.DocDate >= options.DateFrom) &&
             (options.DateTo == null || x.DocDate <= options.DateTo);
}
