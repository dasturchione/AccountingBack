using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Organizations;

public class OrganizationDtoProjection : IProjectionBuilder<Organization, OrganizationDto>
{
    public Expression<Func<Organization, OrganizationDto>> Build()
    {
        return x => new OrganizationDto
        {
            Id           = x.Id,
            ShortName    = x.ShortName,
            FullName     = x.FullName,
            Inn          = x.Inn,
            PhoneNumber  = x.PhoneNumber,
            RegionId     = x.RegionId,
            RegionName   = x.Region.FullName,
            DistrictId   = x.DistrictId,
            DistrictName = x.District != null ? x.District.FullName : null,
            Address      = x.Address,
            Director     = x.Director,
            IsParent     = x.IsParent,
            StateId      = x.StateId,
            StateName    = x.State.FullName,
            CreatedDate  = x.CreatedDate
        };
    }
}
