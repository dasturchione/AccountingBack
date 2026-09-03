using FluentValidation;
using SharedKernel.Constants;

namespace Application.Features.PaymentAcceptancePointOperations;

public sealed class PaymentAcceptancePointOperationBaseDtoValidator : AbstractValidator<PaymentAcceptancePointOperationBaseDto>
{
    public PaymentAcceptancePointOperationBaseDtoValidator()
    {
        RuleFor(x => x.PaymentAcceptancePointId).GreaterThan(0);
        RuleFor(x => x.DirectionId).Must(MovementDirectionIdConst.IsValid);
        RuleFor(x => x.DocDate).NotEmpty();
        RuleFor(x => x.CurrencyId).GreaterThan((short)0);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.ExchangeRate).GreaterThan(0);
        RuleFor(x => x.ExternalTransactionNumber).MaximumLength(150);
        RuleFor(x => x.Comment).MaximumLength(1000);
    }
}
