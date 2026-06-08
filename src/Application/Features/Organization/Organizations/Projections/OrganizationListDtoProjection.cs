using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Organizations;

public class OrganizationListDtoProjection : IProjectionBuilder<Organization, OrganizationListDto>
{
    public Expression<Func<Organization, OrganizationListDto>> Build()
    {
        return x => new OrganizationListDto
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
            Director     = x.Director,
            IsParent     = x.IsParent,
            StateId             = x.StateId,
            StateName           = x.State.FullName,
            DefaultLanguageId   = x.DefaultLanguageId,
            DefaultLanguageName = x.DefaultLanguage != null ? x.DefaultLanguage.Name : null,
            CreatedDate         = x.CreatedDate
        };
    }
}
