using Application.Options;
using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Positions;

public class PositionByIdCriteriaBuilder : ICriteriaBuilder<Position, GetByIdOptions<int>>
{
    public Expression<Func<Position, bool>> Build(GetByIdOptions<int> options) => p => p.Id == options.Id;
}
