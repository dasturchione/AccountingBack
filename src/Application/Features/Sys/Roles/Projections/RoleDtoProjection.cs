using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Roles;

public class RoleDtoProjection : IProjectionBuilder<Role, RoleDto>
{
    public Expression<Func<Role, RoleDto>> Build() =>
        role => new RoleDto
        {
            Id = role.Id,
            ShortName = role.ShortName,
            FullName = role.FullName,
            Code = role.Code,
            Description = role.Description,
            IsSystem = role.IsSystem,
            SortOrder = role.SortOrder,
            StateId = role.StateId,
            StateName = role.State.FullName,
            CreatedDate = role.CreatedDate
        };
}