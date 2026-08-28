using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.CashCollections;

public sealed class CashCollectionDtoProjection : IProjectionBuilder<CashCollectionDoc, CashCollectionDto>
{
    public Expression<Func<CashCollectionDoc, CashCollectionDto>> Build() => x => new()
    {
        Id = x.Id,
        OrganizationId = x.OrganizationId,
        OrganizationName = x.Organization.ShortName,
        DocNumber = x.DocNumber,
        DocDate = x.DocDate,
        CashBoxId = x.CashBoxId,
        CashBoxName = x.CashBox.Name,
        BankAccountId = x.BankAccountId,
        BankAccountNumber = x.BankAccount.AccountNumber,
        BankName = x.BankAccount.Bank.Name,
        CurrencyId = x.CurrencyId,
        CurrencyCode = x.Currency.Code,
        CurrencyName = x.Currency.Name,
        Amount = x.Amount,
        ExchangeRate = x.ExchangeRate,
        CashChartAccountId = x.CashChartAccountId,
        CashChartAccountNumber = x.CashChartAccount == null ? null : x.CashChartAccount.Number,
        CashChartAccountName = x.CashChartAccount == null ? null : x.CashChartAccount.Name,
        CashInTransitAccountId = x.CashInTransitAccountId,
        CashInTransitAccountNumber = x.CashInTransitAccount == null ? null : x.CashInTransitAccount.Number,
        CashInTransitAccountName = x.CashInTransitAccount == null ? null : x.CashInTransitAccount.Name,
        BankChartAccountId = x.BankChartAccountId,
        BankChartAccountNumber = x.BankChartAccount == null ? null : x.BankChartAccount.Number,
        BankChartAccountName = x.BankChartAccount == null ? null : x.BankChartAccount.Name,
        BankOperationId = x.BankOperations
            .Where(operation => operation.StateId == StateIdConst.ACTIVE && operation.StatusId != DocumentStatusIdConst.CANCELLED)
            .OrderByDescending(operation => operation.Id)
            .Select(operation => (long?)operation.Id)
            .FirstOrDefault(),
        StatusId = x.StatusId,
        StatusName = x.Status.Name,
        StateId = x.StateId,
        StateName = x.State.FullName,
        Comment = x.Comment,
        CreatedDate = x.CreatedDate,
        InTransitAt = x.InTransitAt,
        InTransitByUserId = x.InTransitByUserId,
        CompletedAt = x.CompletedAt,
        CompletedByUserId = x.CompletedByUserId,
        CancelledAt = x.CancelledAt,
        CancelledByUserId = x.CancelledByUserId,
        CancelledFromStatusId = x.CancelledFromStatusId
    };
}

public sealed class CashCollectionListDtoProjection : IProjectionBuilder<CashCollectionDoc, CashCollectionListDto>
{
    public Expression<Func<CashCollectionDoc, CashCollectionListDto>> Build()
    {
        return x => new CashCollectionListDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            OrganizationName = x.Organization.ShortName,
            DocNumber = x.DocNumber,
            DocDate = x.DocDate,
            CashBoxId = x.CashBoxId,
            CashBoxName = x.CashBox.Name,
            BankAccountId = x.BankAccountId,
            BankAccountNumber = x.BankAccount.AccountNumber,
            BankName = x.BankAccount.Bank.Name,
            CurrencyId = x.CurrencyId,
            CurrencyCode = x.Currency.Code,
            CurrencyName = x.Currency.Name,
            Amount = x.Amount,
            ExchangeRate = x.ExchangeRate,
            CashChartAccountId = x.CashChartAccountId,
            CashChartAccountNumber = x.CashChartAccount == null ? null : x.CashChartAccount.Number,
            CashChartAccountName = x.CashChartAccount == null ? null : x.CashChartAccount.Name,
            CashInTransitAccountId = x.CashInTransitAccountId,
            CashInTransitAccountNumber = x.CashInTransitAccount == null ? null : x.CashInTransitAccount.Number,
            CashInTransitAccountName = x.CashInTransitAccount == null ? null : x.CashInTransitAccount.Name,
            BankChartAccountId = x.BankChartAccountId,
            BankChartAccountNumber = x.BankChartAccount == null ? null : x.BankChartAccount.Number,
            BankChartAccountName = x.BankChartAccount == null ? null : x.BankChartAccount.Name,
            BankOperationId = x.BankOperations
                .Where(operation => operation.StateId == StateIdConst.ACTIVE && operation.StatusId != DocumentStatusIdConst.CANCELLED)
                .OrderByDescending(operation => operation.Id)
                .Select(operation => (long?)operation.Id)
                .FirstOrDefault(),
            StatusId = x.StatusId,
            StatusName = x.Status.Name,
            StateId = x.StateId,
            StateName = x.State.FullName,
            Comment = x.Comment,
            CreatedDate = x.CreatedDate,
            InTransitAt = x.InTransitAt,
            InTransitByUserId = x.InTransitByUserId,
            CompletedAt = x.CompletedAt,
            CompletedByUserId = x.CompletedByUserId,
            CancelledAt = x.CancelledAt,
            CancelledByUserId = x.CancelledByUserId,
            CancelledFromStatusId = x.CancelledFromStatusId
        };
    }
}

public sealed class CashCollectionInTransitDtoProjection : IProjectionBuilder<CashCollectionDoc, CashCollectionInTransitDto>
{
    public Expression<Func<CashCollectionDoc, CashCollectionInTransitDto>> Build() => x => new()
    {
        Id = x.Id,
        DocNumber = x.DocNumber,
        DocDate = x.DocDate,
        CashBoxId = x.CashBoxId,
        CashBoxName = x.CashBox.Name,
        BankAccountId = x.BankAccountId,
        BankAccountNumber = x.BankAccount.AccountNumber,
        BankName = x.BankAccount.Bank.Name,
        CurrencyId = x.CurrencyId,
        CurrencyCode = x.Currency.Code,
        Amount = x.Amount
    };
}
