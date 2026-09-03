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
        (!filter.DateTo.HasValue || x.StartDate <= filter.DateTo.Value.Date);
}

public sealed class RentalContractListCriteriaBuilder : ICriteriaBuilder<RentalContractListDto, RentalContractListFilter>
{
    public Expression<Func<RentalContractListDto, bool>> Build(RentalContractListFilter filter) => x =>
        string.IsNullOrEmpty(filter.Search) ||
        x.ContractNumber.ToLower().Contains(filter.Search.ToLower()) ||
        x.LessorFullName.ToLower().Contains(filter.Search.ToLower()) ||
        (x.LessorInn != null && x.LessorInn.Contains(filter.Search)) ||
        (x.LessorPinfl != null && x.LessorPinfl.Contains(filter.Search));
}

public sealed class RentalContractOrderByBuilder : IOrderByBuilder<RentalContract, RentalContractListDto>
{
    public Func<IQueryable<RentalContractListDto>, IOrderedQueryable<RentalContractListDto>> Build() =>
        query => query.OrderByDescending(x => x.ContractDate).ThenByDescending(x => x.Id);
}
