using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.CounterpartyContacts;

public class CounterpartyContactListDtoByListFilterCriteriaBuilder : ICriteriaBuilder<CounterpartyContactListDto, CounterpartyContactListFilter>
{
    public Expression<Func<CounterpartyContactListDto, bool>> Build(CounterpartyContactListFilter options)
        => x => string.IsNullOrEmpty(options.Search) ||
                x.FullName.ToLower().Contains(options.Search.ToLower()) ||
                (x.PhoneNumber != null && x.PhoneNumber.ToLower().Contains(options.Search.ToLower()));
}
