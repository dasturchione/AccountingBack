using Application.Options;
using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.CounterpartyContacts;

public class CounterpartyContactByIdCriteriaBuilder : ICriteriaBuilder<CounterpartyContact, GetByIdOptions<int>>
{
    public Expression<Func<CounterpartyContact, bool>> Build(GetByIdOptions<int> options) => x => x.Id == options.Id;
}
