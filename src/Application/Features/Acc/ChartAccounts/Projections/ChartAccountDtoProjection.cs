using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.ChartAccounts;

public class ChartAccountDtoProjection : IProjectionBuilder<ChartAccount, ChartAccountDto>
{
    public Expression<Func<ChartAccount, ChartAccountDto>> Build() =>
        x => new ChartAccountDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            OrganizationName = x.Organization.ShortName,
            ParentId = x.ParentId,
            ParentName = x.Parent != null ? x.Parent.Name : null,
            Code = x.Code,
            Name = x.Name,
            IsGroup = x.IsGroup,
            StateId = x.StateId,
            StateName = x.State.FullName,
            CreatedDate = x.CreatedDate
        };
}
