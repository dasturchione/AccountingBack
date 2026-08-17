using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.Hr.Absences;

public sealed class HrAbsenceListDtoOrderByBuilder : IOrderByBuilder<HrAbsence, HrAbsenceListDto>
{
    public Func<IQueryable<HrAbsenceListDto>, IOrderedQueryable<HrAbsenceListDto>> Build() =>
        query => query.OrderByDescending(x => x.DocDate).ThenByDescending(x => x.Id);
}
