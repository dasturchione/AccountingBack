using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Positions;

public class PositionListDtoProjection : IProjectionBuilder<Position, PositionListDto>
{
    public Expression<Func<Position, PositionListDto>> Build() =>
        x => new PositionListDto
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
