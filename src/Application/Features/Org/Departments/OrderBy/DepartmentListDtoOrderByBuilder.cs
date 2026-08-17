using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.Departments;

public sealed class DepartmentListDtoOrderByBuilder : IOrderByBuilder<Department, DepartmentListDto>
{
    public Func<IQueryable<DepartmentListDto>, IOrderedQueryable<DepartmentListDto>> Build() =>
        query => query.OrderBy(x => x.Name).ThenBy(x => x.Id);
}
