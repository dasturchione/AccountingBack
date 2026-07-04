using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.MoneyRegisterBalances;

public class MoneyRegisterBalanceByListFilterCriteriaBuilder : ICriteriaBuilder<MoneyRegisterBalance, MoneyRegisterBalanceListFilter>
{
    public Expression<Func<MoneyRegisterBalance, bool>> Build(MoneyRegisterBalanceListFilter options)
    {
        var sourceType = options.SourceType?.Trim();
        var sourceTypePattern = string.IsNullOrWhiteSpace(sourceType) ? null : $"{sourceType}%";

        return x => (!options.DocumentTypeId.HasValue || x.DocumentTypeId == options.DocumentTypeId.Value) &&
                    (!options.DocumentId.HasValue || x.DocumentId == options.DocumentId.Value) &&
                    (sourceTypePattern == null || EF.Functions.Like(x.SourceType, sourceTypePattern)) &&
                    (!options.SourceId.HasValue || x.SourceId == options.SourceId.Value) &&
                    (!options.OperationTypeId.HasValue || x.OperationTypeId == options.OperationTypeId.Value) &&
                    (!options.CurrencyId.HasValue || x.CurrencyId == options.CurrencyId.Value) &&
                    (options.DateFrom == null || x.DocDate >= options.DateFrom) &&
                    (options.DateTo == null || x.DocDate <= options.DateTo);
    }
}
