using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.CashFiscalTransfers;

public sealed class CashFiscalTransferDtoProjection : IProjectionBuilder<CashFiscalTransferDoc, CashFiscalTransferDto>
{
    public Expression<Func<CashFiscalTransferDoc, CashFiscalTransferDto>> Build() => x => new()
    {
        Id = x.Id,
        OrganizationId = x.OrganizationId,
        OrganizationName = x.Organization.ShortName,
        DocNumber = x.DocNumber,
        DocDate = x.DocDate,
        FiscalCashRegisterId = x.FiscalCashRegisterId,
        FiscalCashRegisterName = x.FiscalCashRegister.Name,
        CashBoxId = x.CashBoxId,
        CashBoxName = x.CashBox.Name,
        DirectionId = x.DirectionId,
        DirectionCode = x.Direction.Code,
        DirectionName = x.Direction.Name,
        CurrencyId = x.CurrencyId,
        CurrencyName = x.Currency.Name,
        Amount = x.Amount,
        ExchangeRate = x.ExchangeRate,
        FiscalCashAccountId = x.FiscalCashAccountId,
        FiscalCashAccountNumber = x.FiscalCashAccount == null ? null : x.FiscalCashAccount.Number,
        FiscalCashAccountName = x.FiscalCashAccount == null ? null : x.FiscalCashAccount.Name,
        CashBoxAccountId = x.CashBoxAccountId,
        CashBoxAccountNumber = x.CashBoxAccount == null ? null : x.CashBoxAccount.Number,
        CashBoxAccountName = x.CashBoxAccount == null ? null : x.CashBoxAccount.Name,
        StatusId = x.StatusId,
        StatusName = x.Status.Name,
        StateId = x.StateId,
        StateName = x.State.FullName,
        Comment = x.Comment,
        CreatedDate = x.CreatedDate,
        PostedAt = x.PostedAt,
        PostedByUserId = x.PostedByUserId,
        CancelledAt = x.CancelledAt,
        CancelledByUserId = x.CancelledByUserId
    };
}

public sealed class CashFiscalTransferListDtoProjection : IProjectionBuilder<CashFiscalTransferDoc, CashFiscalTransferListDto>
{
    public Expression<Func<CashFiscalTransferDoc, CashFiscalTransferListDto>> Build() => x => new()
    {
        Id = x.Id,
        OrganizationId = x.OrganizationId,
        OrganizationName = x.Organization.ShortName,
        DocNumber = x.DocNumber,
        DocDate = x.DocDate,
        FiscalCashRegisterId = x.FiscalCashRegisterId,
        FiscalCashRegisterName = x.FiscalCashRegister.Name,
        CashBoxId = x.CashBoxId,
        CashBoxName = x.CashBox.Name,
        DirectionId = x.DirectionId,
        DirectionCode = x.Direction.Code,
        DirectionName = x.Direction.Name,
        CurrencyId = x.CurrencyId,
        CurrencyName = x.Currency.Name,
        Amount = x.Amount,
        ExchangeRate = x.ExchangeRate,
        FiscalCashAccountId = x.FiscalCashAccountId,
        FiscalCashAccountNumber = x.FiscalCashAccount == null ? null : x.FiscalCashAccount.Number,
        FiscalCashAccountName = x.FiscalCashAccount == null ? null : x.FiscalCashAccount.Name,
        CashBoxAccountId = x.CashBoxAccountId,
        CashBoxAccountNumber = x.CashBoxAccount == null ? null : x.CashBoxAccount.Number,
        CashBoxAccountName = x.CashBoxAccount == null ? null : x.CashBoxAccount.Name,
        StatusId = x.StatusId,
        StatusName = x.Status.Name,
        StateId = x.StateId,
        StateName = x.State.FullName,
        Comment = x.Comment,
        CreatedDate = x.CreatedDate,
        PostedAt = x.PostedAt,
        PostedByUserId = x.PostedByUserId,
        CancelledAt = x.CancelledAt,
        CancelledByUserId = x.CancelledByUserId
    };
}
