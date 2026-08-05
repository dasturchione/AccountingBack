using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Roles;

public class RoleListDtoProjection : IProjectionBuilder<Role, RoleListDto>
{
    public Expression<Func<Role, RoleListDto>> Build() =>
        role => new RoleListDto
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