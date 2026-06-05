using Application.Abstractions;
using Domain.Entities;
using System.Linq.Expressions;

namespace Application.Features.Roles;

public class RoleDtoMap : IProjectionMap<Role, RoleDto>
{
    public Expression<Func<Role, RoleDto>> Build()
    {
        return x => new RoleDto
        {
            Id          = x.Id,
            ShortName   = x.ShortName,
            FullName    = x.FullName,
            StateId     = x.StateId,
            StateName   = x.State.FullName,
            CreatedDate = x.CreatedDate
        };
    }
}
