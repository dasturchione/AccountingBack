using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.CashOperations;

public class CashOperationListDtoProjection : IProjectionBuilder<CashOperation, CashOperationListDto>
{
    public Expression<Func<CashOperation, CashOperationListDto>> Build() =>
        x => new CashOperationListDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            OrganizationName = x.Organization.ShortName,
            CashBoxId = x.CashBoxId,
            CashBoxName = x.CashBox.Name,
            DestinationCashBoxId = x.DestinationCashBoxId,
            DestinationCashBoxName = x.DestinationCashBox != null ? x.DestinationCashBox.Name : null,
            OperationTypeId = x.OperationTypeId,
            OperationTypeName = x.OperationType.Name,
            PaymentTypeId = x.PaymentTypeId,
            PaymentTypeName = x.PaymentType != null ? x.PaymentType.Name : null,
            CashChartAccountId = x.CashChartAccountId,
            CashChartAccountNumber = x.CashChartAccount == null ? null : x.CashChartAccount.Number,
            CashChartAccountName = x.CashChartAccount == null ? null : x.CashChartAccount.Name,
            OffsetAccountId = x.OffsetAccountId,
            OffsetAccountNumber = x.OffsetAccount == null ? null : x.OffsetAccount.Number,
            OffsetAccountName = x.OffsetAccount == null ? null : x.OffsetAccount.Name,
            CounterpartyId = x.CounterpartyId,
            CounterpartyName = x.Counterparty != null ? x.Counterparty.ShortName : null,
            DocNumber = x.DocNumber,
            DocDate = x.DocDate,
            CurrencyId = x.CurrencyId,
            CurrencyName = x.Currency.Name,
            Amount = x.Amount,
            ExchangeRate = x.ExchangeRate,
            PostedAt = x.PostedAt,
            PostedByUserId = x.PostedByUserId,
            CancelledAt = x.CancelledAt,
            CancelledByUserId = x.CancelledByUserId,
            Comment = x.Comment,
            StatusId = x.StatusId,
            StatusName = x.Status.Name,
            StateId = x.StateId,
            StateName = x.State.FullName,
            CreatedDate = x.CreatedDate
        };
}
