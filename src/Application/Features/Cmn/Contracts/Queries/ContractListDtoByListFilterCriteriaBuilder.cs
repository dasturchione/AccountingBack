using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Contracts;

public class ContractListDtoByListFilterCriteriaBuilder : ICriteriaBuilder<ContractListDto, ContractListFilter>
{
    public Expression<Func<ContractListDto, bool>> Build(ContractListFilter options)
        => x => string.IsNullOrEmpty(options.Search) ||
                x.ContractNumber.ToLower().Contains(options.Search.ToLower()) ||
                x.CounterpartyName.ToLower().Contains(options.Search.ToLower());
}
