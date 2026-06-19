using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Departments;

public class DepartmentByListFilterCriteriaBuilder : ICriteriaBuilder<Department, DepartmentListFilter>
{
    public Expression<Func<Department, bool>> Build(DepartmentListFilter options)
        => d => (options.BranchId == null || d.BranchId == options.BranchId);
}
