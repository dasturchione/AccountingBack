using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.MoneyRegisterBalances;

public class MoneyRegisterBalanceByListFilterCriteriaBuilder : ICriteriaBuilder<MoneyRegisterBalance, MoneyRegisterBalanceListFilter>
{
    public Expression<Func<MoneyRegisterBalance, bool>> Build(MoneyRegisterBalanceListFilter options) =>
        x => (!options.OrganizationId.HasValue || x.OrganizationId == options.OrganizationId.Value) &&
             (!options.DocumentTypeId.HasValue || x.DocumentTypeId == options.DocumentTypeId.Value) &&
             (!options.DocumentId.HasValue || x.DocumentId == options.DocumentId.Value) &&
             (string.IsNullOrEmpty(options.SourceType) || x.SourceType.ToLower().Contains(options.SourceType.ToLower())) &&
             (!options.SourceId.HasValue || x.SourceId == options.SourceId.Value) &&
             (!options.OperationTypeId.HasValue || x.OperationTypeId == options.OperationTypeId.Value) &&
             (!options.CurrencyId.HasValue || x.CurrencyId == options.CurrencyId.Value) &&
             (options.DateFrom == null || x.DocDate >= options.DateFrom) &&
             (options.DateTo == null || x.DocDate <= options.DateTo);
}
