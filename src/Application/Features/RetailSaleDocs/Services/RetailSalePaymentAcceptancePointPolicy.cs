using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.RetailSaleDocs;

public sealed record RetailSalePaymentAcceptancePointMovement(short DirectionId, short StatusId);

public static class RetailSalePaymentAcceptancePointPolicy
{
    private static readonly IReadOnlyList<RetailSalePaymentAcceptancePointMovement> NoMovements =
        Array.Empty<RetailSalePaymentAcceptancePointMovement>();

    private static readonly IReadOnlyList<RetailSalePaymentAcceptancePointMovement> IncomingOnly =
        new[]
        {
            new RetailSalePaymentAcceptancePointMovement(
                MovementDirectionIdConst.IN,
                DocumentStatusIdConst.POSTED)
        };

    private static readonly IReadOnlyList<RetailSalePaymentAcceptancePointMovement> CardMovements =
        new[]
        {
            new RetailSalePaymentAcceptancePointMovement(
                MovementDirectionIdConst.IN,
                DocumentStatusIdConst.POSTED),
            new RetailSalePaymentAcceptancePointMovement(
                MovementDirectionIdConst.OUT,
                DocumentStatusIdConst.PENDING)
        };

    public static Result<IReadOnlyList<RetailSalePaymentAcceptancePointMovement>> Build(
        string paymentMethodCode,
        int? paymentAcceptancePointId,
        short? languageId = null)
    {
        if (paymentMethodCode == PaymentMethodCodeConst.CASH)
        {
            return paymentAcceptancePointId.HasValue
                ? Result.Failure<IReadOnlyList<RetailSalePaymentAcceptancePointMovement>>(
                    RetailSaleDocErrors.InvalidPayment(languageId))
                : Result.Success(NoMovements);
        }

        if (!paymentAcceptancePointId.HasValue)
        {
            return Result.Failure<IReadOnlyList<RetailSalePaymentAcceptancePointMovement>>(
                RetailSaleDocErrors.InvalidPayment(languageId));
        }

        return Result.Success(
            paymentMethodCode == PaymentMethodCodeConst.CARD
                ? CardMovements
                : IncomingOnly);
    }
}
