using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Cmn.CurrencyRevaluations;

public sealed class CurrencyRevaluationListDtoCriteriaBuilder : ICriteriaBuilder<CurrencyRevaluationListDto, CurrencyRevaluationListFilter>
{
    public Expression<Func<CurrencyRevaluationListDto, bool>> Build(CurrencyRevaluationListFilter options) =>
        x => (options.RevaluationFrom == null || x.RevaluationDate >= options.RevaluationFrom) &&
             (options.RevaluationTo == null || x.RevaluationDate <= options.RevaluationTo) &&
             (string.IsNullOrWhiteSpace(options.Search) || x.Id.ToString().Contains(options.Search));
}
