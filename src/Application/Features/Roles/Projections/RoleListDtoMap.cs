using Application.Abstractions;
using Domain.Entities;
using System.Linq.Expressions;

namespace Application.Features.Roles;

public class RoleListDtoMap : IProjectionMap<Role, RoleListDto>
{
    public Expression<Func<Role, RoleListDto>> Build()
    {
        return x => new RoleListDto
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
