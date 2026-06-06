using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Branches;

public class BranchListDtoProjection : IProjectionBuilder<Branch, BranchListDto>
{
    public Expression<Func<Branch, BranchListDto>> Build() =>
        x => new BranchListDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            OrganizationName = x.Organization.ShortName,
            Code = x.Code,
            Name = x.Name,
            RegionId = x.RegionId,
            RegionName = x.Region != null ? x.Region.FullName : null,
            DistrictId = x.DistrictId,
            DistrictName = x.District != null ? x.District.FullName : null,
            PhoneNumber = x.PhoneNumber,
            StateId = x.StateId,
            StateName = x.State.FullName,
            CreatedDate = x.CreatedDate
        };
}
