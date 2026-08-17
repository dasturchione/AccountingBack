using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.Pay.Employees;

public sealed class PayrollEmployeeListDtoOrderByBuilder : IOrderByBuilder<PayEmployee, PayrollEmployeeListDto>
{
    public Func<IQueryable<PayrollEmployeeListDto>, IOrderedQueryable<PayrollEmployeeListDto>> Build() =>
        query => query.OrderBy(x => x.FullName).ThenBy(x => x.Id);
}
