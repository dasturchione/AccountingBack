using Application.Options;
using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.CashBoxes;

public class CashBoxByIdCriteriaBuilder : ICriteriaBuilder<CashBox, GetByIdOptions<int>>
{
    public Expression<Func<CashBox, bool>> Build(GetByIdOptions<int> options) => x => x.Id == options.Id;
}
