using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.ChartAccounts;

public class ChartAccountListDtoProjection : IProjectionBuilder<ChartAccount, ChartAccountListDto>
{
    public Expression<Func<ChartAccount, ChartAccountListDto>> Build() =>
        x => new ChartAccountListDto
        {
            Id = x.Id,
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
