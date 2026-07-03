using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.CashOperations;

public class CashOperationDtoProjection : IProjectionBuilder<CashOperation, CashOperationDto>
{
    public Expression<Func<CashOperation, CashOperationDto>> Build() =>
        x => new CashOperationDto
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
