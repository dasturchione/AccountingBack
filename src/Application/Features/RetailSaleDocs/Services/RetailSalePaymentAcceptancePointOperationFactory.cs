using Domain.Entities;
using SharedKernel.Constants;

namespace Application.Features.RetailSaleDocs;

public static class RetailSalePaymentAcceptancePointOperationFactory
{
    public static PaymentAcceptancePointOperation Create(
        int organizationId,
        int paymentAcceptancePointId,
        long relatedDocumentId,
        string docNumber,
        DateTime docDate,
        short currencyId,
        decimal amount,
        decimal exchangeRate,
        string? transactionNumber,
        RetailSalePaymentAcceptancePointMovement movement,
        int? userId,
        DateTime now) =>
        new()
        {
            OrganizationId = organizationId,
            PaymentAcceptancePointId = paymentAcceptancePointId,
            RelatedDocumentId = relatedDocumentId,
            DirectionId = movement.DirectionId,
            DocNumber = docNumber,
            DocDate = docDate,
            CurrencyId = currencyId,
            Amount = amount,
            ExchangeRate = exchangeRate,
            ExternalTransactionNumber = string.IsNullOrWhiteSpace(transactionNumber)
                ? null
                : transactionNumber.Trim(),
            StatusId = movement.StatusId,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = now,
            PostedAt = movement.StatusId == DocumentStatusIdConst.POSTED ? now : null,
            PostedByUserId = movement.StatusId == DocumentStatusIdConst.POSTED ? userId : null,
            Comment = "Created from retail sale payment"
        };
}
