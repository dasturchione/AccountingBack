using Application.Options;
using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.CounterpartyCards;

public class CounterpartyCardByIdCriteriaBuilder : ICriteriaBuilder<CounterpartyCard, GetByIdOptions<int>>
{
    public Expression<Func<CounterpartyCard, bool>> Build(GetByIdOptions<int> options) => x => x.Id == options.Id;
}
