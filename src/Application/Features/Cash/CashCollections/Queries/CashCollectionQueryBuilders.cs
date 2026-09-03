using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.CashCollections;

public sealed class CashCollectionCriteriaBuilder : ICriteriaBuilder<CashCollectionDoc, CashCollectionListFilter>
{
    public Expression<Func<CashCollectionDoc, bool>> Build(CashCollectionListFilter filter) => x =>
        x.StateId == StateIdConst.ACTIVE &&
        (!filter.CashBoxId.HasValue || x.CashBoxId == filter.CashBoxId.Value) &&
        (!filter.BankAccountId.HasValue || x.BankAccountId == filter.BankAccountId.Value) &&
        (!filter.CurrencyId.HasValue || x.CurrencyId == filter.CurrencyId.Value) &&
        (!filter.StatusId.HasValue || x.StatusId == filter.StatusId.Value) &&
        (!filter.DateFrom.HasValue || x.DocDate >= filter.DateFrom.Value) &&
        (!filter.DateTo.HasValue || x.DocDate <= filter.DateTo.Value);
}

public sealed class CashCollectionListCriteriaBuilder : ICriteriaBuilder<CashCollectionListDto, CashCollectionListFilter>
{
    public Expression<Func<CashCollectionListDto, bool>> Build(CashCollectionListFilter filter) => x =>
        string.IsNullOrEmpty(filter.Search) ||
        x.DocNumber.ToLower().Contains(filter.Search.ToLower()) ||
        x.CashBoxName.ToLower().Contains(filter.Search.ToLower()) ||
        x.BankAccountNumber.ToLower().Contains(filter.Search.ToLower()) ||
        x.BankName.ToLower().Contains(filter.Search.ToLower());
}

public sealed class CashCollectionListOrderByBuilder : IOrderByBuilder<CashCollectionDoc, CashCollectionListDto>
{
    public Func<IQueryable<CashCollectionListDto>, IOrderedQueryable<CashCollectionListDto>> Build() =>
        query => query.OrderByDescending(x => x.DocDate).ThenByDescending(x => x.Id);
}
