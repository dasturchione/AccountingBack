using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Positions;

public class PositionListDtoByListFilterCriteriaBuilder : ICriteriaBuilder<PositionListDto, PositionListFilter>
{
    public Expression<Func<PositionListDto, bool>> Build(PositionListFilter options)
        => x => string.IsNullOrEmpty(options.Search) ||
                x.Name.ToLower().Contains(options.Search.ToLower()) ||
                x.Code.ToLower().Contains(options.Search.ToLower());
}
