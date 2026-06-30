using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.CounterpartyCards;

public class CounterpartyCardListDtoProjection : IProjectionBuilder<CounterpartyCard, CounterpartyCardListDto>
{
    public Expression<Func<CounterpartyCard, CounterpartyCardListDto>> Build() =>
        x => new CounterpartyCardListDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            OrganizationName = x.Organization.ShortName,
            CounterpartyTypeId = x.CounterpartyTypeId,
            CounterpartyTypeName = x.CounterpartyType.Name,
            Code = x.Code,
            IsCustomer = x.IsCustomer,
            IsSupplier = x.IsSupplier,
            IsVatPayer = x.IsVatPayer,
            ShortName = x.ShortName,
            FullName = x.FullName,
            Inn = x.Inn,
            Email = x.Email,
            PhoneNumber = x.PhoneNumber,
            RegionId = x.RegionId,
            RegionName = x.Region != null ? x.Region.FullName : null,
            DistrictId = x.DistrictId,
            DistrictName = x.District != null ? x.District.FullName : null,
            StateId = x.StateId,
            Address = x.Address,
            StateName = x.State.FullName,
            Oked = x.Oked,
            ExternalId = x.ExternalId,
            CreatedDate = x.CreatedDate
        };
}
