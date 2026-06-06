using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Positions;

public class PositionDtoProjection : IProjectionBuilder<Position, PositionDto>
{
    public Expression<Func<Position, PositionDto>> Build() =>
        x => new PositionDto
        {
            Id               = x.Id,
            OrganizationId   = x.OrganizationId,
            OrganizationName = x.Organization.ShortName,
            Code             = x.Code,
            Name             = x.Name,
            StateId          = x.StateId,
            StateName        = x.State.FullName,
            CreatedDate      = x.CreatedDate
        };
}
