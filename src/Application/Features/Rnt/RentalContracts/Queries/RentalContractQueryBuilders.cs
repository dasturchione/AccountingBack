using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Rnt.RentalContracts;

public sealed class RentalContractCriteriaBuilder : ICriteriaBuilder<RentalContract, RentalContractListFilter>
{
    public Expression<Func<RentalContract, bool>> Build(RentalContractListFilter filter) => x =>
        x.StateId == StateIdConst.ACTIVE &&
        (!filter.StatusId.HasValue || x.StatusId == filter.StatusId.Value) &&
        (!filter.DateFrom.HasValue || x.EndDate >= filter.DateFrom.Value.Date) &&
        (!filter.DateTo.HasValue || x.StartDate <= filter.DateTo.Value.Date) &&
        (string.IsNullOrEmpty(filter.Search) ||
         x.ContractNumber.ToLower().Contains(filter.Search.ToLower()) ||
         x.Lessors.Any(link =>
             link.Lessor.FullName.ToLower().Contains(filter.Search.ToLower()) ||
             (link.Lessor.Inn != null && link.Lessor.Inn.Contains(filter.Search)) ||
             (link.Lessor.Pinfl != null && link.Lessor.Pinfl.Contains(filter.Search))));
}

public sealed class RentalContractOrderByBuilder : IOrderByBuilder<RentalContract, RentalContractListDto>
{
    public Func<IQueryable<RentalContractListDto>, IOrderedQueryable<RentalContractListDto>> Build() =>
        query => query.OrderByDescending(x => x.ContractDate).ThenByDescending(x => x.Id);
}
