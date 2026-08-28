using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.PaymentAcceptancePointOperations;

public sealed class PaymentAcceptancePointOperationListDtoProjection
    : IProjectionBuilder<PaymentAcceptancePointOperation, PaymentAcceptancePointOperationListDto>
{
    public Expression<Func<PaymentAcceptancePointOperation, PaymentAcceptancePointOperationListDto>> Build() =>
        x => new PaymentAcceptancePointOperationListDto
        {
            Id = x.Id,
            PaymentAcceptancePointId = x.PaymentAcceptancePointId,
            PaymentAcceptancePointCode = x.PaymentAcceptancePoint.Code,
            PaymentAcceptancePointName = x.PaymentAcceptancePoint.Name,
            DirectionId = x.DirectionId,
            DirectionName = x.Direction.Name,
            DocNumber = x.DocNumber,
            DocDate = x.DocDate,
            CurrencyId = x.CurrencyId,
            CurrencyCode = x.Currency.Code,
            Amount = x.Amount,
            ExternalTransactionNumber = x.ExternalTransactionNumber,
            RelatedDocumentId = x.RelatedDocumentId,
            RelatedDocumentNumber = x.RelatedDocument == null ? null : x.RelatedDocument.DocNumber,
            StatusId = x.StatusId,
            StatusName = x.Status.Name,
            StateId = x.StateId,
            CreatedDate = x.CreatedDate
        };
}
