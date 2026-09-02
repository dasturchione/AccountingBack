using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.CounterpartyContacts;

public class CounterpartyContactByListFilterCriteriaBuilder : ICriteriaBuilder<CounterpartyContact, CounterpartyContactListFilter>
{
    public Expression<Func<CounterpartyContact, bool>> Build(CounterpartyContactListFilter options) =>
        x => (!options.OrganizationId.HasValue || x.OrganizationId == options.OrganizationId.Value) &&
             (!options.CounterpartyId.HasValue || x.CounterpartyId == options.CounterpartyId.Value);
}
