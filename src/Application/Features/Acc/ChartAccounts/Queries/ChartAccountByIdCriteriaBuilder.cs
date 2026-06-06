using Application.Options;
using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.ChartAccounts;

public class ChartAccountByIdCriteriaBuilder : ICriteriaBuilder<ChartAccount, GetByIdOptions<int>>
{
    public Expression<Func<ChartAccount, bool>> Build(GetByIdOptions<int> options) => x => x.Id == options.Id;
}
