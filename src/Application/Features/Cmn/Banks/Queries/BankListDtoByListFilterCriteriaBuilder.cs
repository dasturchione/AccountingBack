using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Banks;

public class BankListDtoByListFilterCriteriaBuilder : ICriteriaBuilder<BankListDto, BankListFilter>
{
    public Expression<Func<BankListDto, bool>> Build(BankListFilter options) =>
        x => string.IsNullOrEmpty(options.Search) ||
             x.Code.ToLower().Contains(options.Search.ToLower()) ||
             x.Name.ToLower().Contains(options.Search.ToLower()) ||
             (x.LegalName != null && x.LegalName.ToLower().Contains(options.Search.ToLower())) ||
             (x.Inn != null && x.Inn.ToLower().Contains(options.Search.ToLower()));
}
