using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.CounterpartyCards;

public class CounterpartyCardDtoProjection : IProjectionBuilder<CounterpartyCard, CounterpartyCardDto>
{
    public Expression<Func<CounterpartyCard, CounterpartyCardDto>> Build() =>
        x => new CounterpartyCardDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            OrganizationName = x.Organization.ShortName,
            CounterpartyTypeId = x.CounterpartyTypeId,
            CounterpartyTypeName = x.CounterpartyType.Name,
            ShortName = x.ShortName,
            FullName = x.FullName,
            Inn = x.Inn,
            PhoneNumber = x.PhoneNumber,
            Email = x.Email,
            RegionId = x.RegionId,
            RegionName = x.Region != null ? x.Region.FullName : null,
            DistrictId = x.DistrictId,
            DistrictName = x.District != null ? x.District.FullName : null,
            Address = x.Address,
            StateId = x.StateId,
            StateName = x.State.FullName,
            CreatedDate = x.CreatedDate
        };
}
