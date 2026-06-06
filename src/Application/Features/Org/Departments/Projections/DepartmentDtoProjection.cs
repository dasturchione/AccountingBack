using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Departments;

public class DepartmentDtoProjection : IProjectionBuilder<Department, DepartmentDto>
{
    public Expression<Func<Department, DepartmentDto>> Build()
    {
        return x => new DepartmentDto
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
