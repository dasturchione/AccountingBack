using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Departments;

public class DepartmentListDtoByListFilterCriteriaBuilder : ICriteriaBuilder<DepartmentListDto, DepartmentListFilter>
{
    public Expression<Func<DepartmentListDto, bool>> Build(DepartmentListFilter options)
        => x => string.IsNullOrEmpty(options.Search) ||
                x.Name.ToLower().Contains(options.Search.ToLower()) ||
                x.Code.ToLower().Contains(options.Search.ToLower());
}
