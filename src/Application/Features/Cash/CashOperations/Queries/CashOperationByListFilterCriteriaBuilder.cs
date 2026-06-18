using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.CashOperations;

public class CashOperationByListFilterCriteriaBuilder : ICriteriaBuilder<CashOperation, CashOperationListFilter>
{
    public Expression<Func<CashOperation, bool>> Build(CashOperationListFilter options) =>
        x => (!options.CashBoxId.HasValue || x.CashBoxId == options.CashBoxId.Value) &&
             (!options.OperationTypeId.HasValue || x.OperationTypeId == options.OperationTypeId.Value) &&
             (options.DateFrom == null || x.DocDate >= options.DateFrom) &&
             (options.DateTo == null || x.DocDate <= options.DateTo);
}
