using Domain.Entities;
using SharedKernel.Query;
using SharedKernel.Constants;
using System.Linq.Expressions;

namespace Application.Features.CashFiscalTransfers;

public sealed class CashFiscalTransferCriteriaBuilder : ICriteriaBuilder<CashFiscalTransferDoc, CashFiscalTransferListFilter>
{
    public Expression<Func<CashFiscalTransferDoc, bool>> Build(CashFiscalTransferListFilter f) => x =>
        x.StateId == StateIdConst.ACTIVE &&
        (!f.FiscalCashRegisterId.HasValue || x.FiscalCashRegisterId == f.FiscalCashRegisterId) &&
        (!f.CashBoxId.HasValue || x.CashBoxId == f.CashBoxId) &&
        (!f.DirectionId.HasValue || x.DirectionId == f.DirectionId) &&
        (!f.CurrencyId.HasValue || x.CurrencyId == f.CurrencyId) &&
        (!f.StatusId.HasValue || x.StatusId == f.StatusId) &&
        (!f.DateFrom.HasValue || x.DocDate >= f.DateFrom) &&
        (!f.DateTo.HasValue || x.DocDate <= f.DateTo);
}

public sealed class CashFiscalTransferListCriteriaBuilder : ICriteriaBuilder<CashFiscalTransferListDto, CashFiscalTransferListFilter>
{
    public Expression<Func<CashFiscalTransferListDto, bool>> Build(CashFiscalTransferListFilter f) => x =>
        string.IsNullOrEmpty(f.Search) ||
        x.DocNumber.ToLower().Contains(f.Search.ToLower()) ||
        x.FiscalCashRegisterName.ToLower().Contains(f.Search.ToLower()) ||
        x.CashBoxName.ToLower().Contains(f.Search.ToLower());
}

public sealed class CashFiscalTransferListOrderByBuilder : IOrderByBuilder<CashFiscalTransferDoc, CashFiscalTransferListDto>
{
    public Func<IQueryable<CashFiscalTransferListDto>, IOrderedQueryable<CashFiscalTransferListDto>> Build() =>
        query => query.OrderByDescending(x => x.DocDate).ThenByDescending(x => x.Id);
}
