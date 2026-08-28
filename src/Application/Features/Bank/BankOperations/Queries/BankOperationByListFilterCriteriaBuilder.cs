using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.BankOperations;

public class BankOperationByListFilterCriteriaBuilder : ICriteriaBuilder<BankOperation, BankOperationListFilter>
{
    public Expression<Func<BankOperation, bool>> Build(BankOperationListFilter options) =>
        x => (!options.BankAccountId.HasValue || x.BankAccountId == options.BankAccountId.Value) &&
             (!options.DirectionId.HasValue || x.DirectionId == options.DirectionId.Value) &&
             (!options.CashCollectionDocId.HasValue || x.CashCollectionDocId == options.CashCollectionDocId.Value) &&
             (options.DateFrom == null || x.DocDate >= options.DateFrom) &&
             (options.DateTo == null || x.DocDate <= options.DateTo);
}
