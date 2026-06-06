using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Positions;

public class PositionByListFilterCriteriaBuilder : ICriteriaBuilder<Position, PositionListFilter>
{
    public Expression<Func<Position, bool>> Build(PositionListFilter options)
        => p => options.OrganizationId == null || p.OrganizationId == options.OrganizationId;
}
