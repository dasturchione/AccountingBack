using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Rnt.RentalAccruals;

public sealed class RentalAccrualCriteriaBuilder : ICriteriaBuilder<RentalAccrualDoc, RentalAccrualListFilter>
{
    public Expression<Func<RentalAccrualDoc, bool>> Build(RentalAccrualListFilter filter) => x =>
        x.StateId == StateIdConst.ACTIVE &&
        (!filter.ContractId.HasValue || x.ContractId == filter.ContractId.Value) &&
        (!filter.StatusId.HasValue || x.StatusId == filter.StatusId.Value) &&
        (!filter.DateFrom.HasValue || x.DocDate >= filter.DateFrom.Value.Date) &&
        (!filter.DateTo.HasValue || x.DocDate < filter.DateTo.Value.Date.AddDays(1)) &&
        (string.IsNullOrEmpty(filter.Search) ||
         x.DocNumber.ToLower().Contains(filter.Search.ToLower()) ||
         x.Contract.ContractNumber.ToLower().Contains(filter.Search.ToLower()) ||
         x.Contract.Lessors.Any(link =>
             link.Lessor.FullName.ToLower().Contains(filter.Search.ToLower()) ||
             (link.Lessor.Inn != null && link.Lessor.Inn.Contains(filter.Search)) ||
             (link.Lessor.Pinfl != null && link.Lessor.Pinfl.Contains(filter.Search))));
}

public sealed class RentalAccrualOrderByBuilder : IOrderByBuilder<RentalAccrualDoc, RentalAccrualDocListDto>
{
    public Func<IQueryable<RentalAccrualDocListDto>, IOrderedQueryable<RentalAccrualDocListDto>> Build() =>
        query => query.OrderByDescending(x => x.DocDate).ThenByDescending(x => x.Id);
}
