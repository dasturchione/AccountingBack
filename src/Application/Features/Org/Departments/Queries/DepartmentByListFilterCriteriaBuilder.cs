using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Departments;

public class DepartmentByListFilterCriteriaBuilder : ICriteriaBuilder<Department, DepartmentListFilter>
{
    public Expression<Func<Department, bool>> Build(DepartmentListFilter options)
        => d => (!options.OrganizationId.HasValue || d.OrganizationId == options.OrganizationId.Value) &&
                (!options.BranchId.HasValue || d.BranchId == options.BranchId.Value);
}
