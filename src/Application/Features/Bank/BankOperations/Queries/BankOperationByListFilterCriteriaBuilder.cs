using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.BankOperations;

public class BankOperationByListFilterCriteriaBuilder : ICriteriaBuilder<BankOperation, BankOperationListFilter>
{
    public Expression<Func<BankOperation, bool>> Build(BankOperationListFilter options) =>
        x => (!options.OrganizationId.HasValue || x.OrganizationId == options.OrganizationId.Value) &&
             (!options.BankAccountId.HasValue || x.BankAccountId == options.BankAccountId.Value) &&
             (!options.OperationTypeId.HasValue || x.OperationTypeId == options.OperationTypeId.Value) &&
             (options.DateFrom == null || x.DocDate >= options.DateFrom) &&
             (options.DateTo == null || x.DocDate <= options.DateTo);
}
