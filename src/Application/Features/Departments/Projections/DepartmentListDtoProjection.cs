using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Departments;

public class DepartmentListDtoProjection : IProjectionBuilder<Department, DepartmentListDto>
{
    public Expression<Func<Department, DepartmentListDto>> Build()
    {
        return x => new DepartmentListDto
        {
            Id               = x.Id,
            OrganizationId   = x.OrganizationId,
            OrganizationName = x.Organization.ShortName,
            BranchId         = x.BranchId,
            BranchName       = x.Branch != null ? x.Branch.Name : null,
            Code             = x.Code,
            Name             = x.Name,
            StateId          = x.StateId,
            StateName        = x.State.FullName,
            CreatedDate      = x.CreatedDate
        };
    }
}
