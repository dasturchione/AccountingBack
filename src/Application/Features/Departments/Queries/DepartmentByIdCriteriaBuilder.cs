using Application.Options;
using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Departments;

public class DepartmentByIdCriteriaBuilder : ICriteriaBuilder<Department, GetByIdOptions<int>>
{
    public Expression<Func<Department, bool>> Build(GetByIdOptions<int> options)
        => d => d.Id == options.Id;
}
