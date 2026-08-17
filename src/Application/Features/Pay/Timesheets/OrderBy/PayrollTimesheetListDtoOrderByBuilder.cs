using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.Pay.Timesheets;

public sealed class PayrollTimesheetListDtoOrderByBuilder : IOrderByBuilder<PayTimesheet, PayrollTimesheetListDto>
{
    public Func<IQueryable<PayrollTimesheetListDto>, IOrderedQueryable<PayrollTimesheetListDto>> Build() =>
        query => query.OrderByDescending(x => x.DocDate).ThenByDescending(x => x.Id);
}
