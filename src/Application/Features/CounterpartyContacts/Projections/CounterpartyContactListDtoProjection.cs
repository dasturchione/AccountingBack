using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.CounterpartyContacts;

public class CounterpartyContactListDtoProjection : IProjectionBuilder<CounterpartyContact, CounterpartyContactListDto>
{
    public Expression<Func<CounterpartyContact, CounterpartyContactListDto>> Build() =>
        x => new CounterpartyContactListDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            CounterpartyId = x.CounterpartyId,
            CounterpartyName = x.Counterparty.ShortName,
            FullName = x.FullName,
            PhoneNumber = x.PhoneNumber,
            Email = x.Email,
            Position = x.Position,
            Comment = x.Comment,
            StateId = x.StateId,
            StateName = x.State.FullName,
            CreatedDate = x.CreatedDate
        };
}
