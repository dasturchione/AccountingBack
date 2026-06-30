using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Roles;

public class RoleListDtoProjection : IProjectionBuilder<Role, RoleListDto>
{
    public Expression<Func<Role, RoleListDto>> Build()
    {
        return x => new RoleListDto
        {
            Id          = x.Id,
            ShortName   = x.ShortName,
            FullName    = x.FullName,
            Code        = x.Code,
            Description = x.Description,
            HasGlobalAccess = x.HasGlobalAccess,
            IsSystem    = x.IsSystem,
            IsOwnerRole = x.IsOwnerRole,
            SortOrder   = x.SortOrder,
            StateId     = x.StateId,
            StateName   = x.State.FullName,
            CreatedDate = x.CreatedDate
        };
    }
}
