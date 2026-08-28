using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.PaymentAcceptancePointOperations;

public sealed class PaymentAcceptancePointOperationDtoProjection
    : IProjectionBuilder<PaymentAcceptancePointOperation, PaymentAcceptancePointOperationDto>
{
    public Expression<Func<PaymentAcceptancePointOperation, PaymentAcceptancePointOperationDto>> Build() =>
        x => new PaymentAcceptancePointOperationDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            OrganizationName = x.Organization.ShortName,
            PaymentAcceptancePointId = x.PaymentAcceptancePointId,
            PaymentAcceptancePointCode = x.PaymentAcceptancePoint.Code,
            PaymentAcceptancePointName = x.PaymentAcceptancePoint.Name,
            DirectionId = x.DirectionId,
            DirectionName = x.Direction.Name,
            DocNumber = x.DocNumber,
            DocDate = x.DocDate,
            CurrencyId = x.CurrencyId,
            CurrencyCode = x.Currency.Code,
            CurrencyName = x.Currency.Name,
            Amount = x.Amount,
            ExchangeRate = x.ExchangeRate,
            ExternalTransactionNumber = x.ExternalTransactionNumber,
            Comment = x.Comment,
            StatusId = x.StatusId,
            StatusName = x.Status.Name,
            StateId = x.StateId,
            StateName = x.State.FullName,
            CreatedDate = x.CreatedDate,
            PostedAt = x.PostedAt,
            PostedByUserId = x.PostedByUserId,
            CancelledAt = x.CancelledAt,
            CancelledByUserId = x.CancelledByUserId
        };
}
