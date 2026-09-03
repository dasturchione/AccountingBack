using FluentValidation;

namespace Application.Features.PaymentAcceptancePoints;

public class PaymentAcceptancePointBaseDtoValidator : AbstractValidator<PaymentAcceptancePointBaseDto>
{
    public PaymentAcceptancePointBaseDtoValidator()
    {
        RuleFor(x => x.TypeId).GreaterThan((short)0);
        RuleFor(x => x.BankAccountId)
            .GreaterThan(0)
            .When(x => x.BankAccountId.HasValue);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
        RuleFor(x => x.MerchantId).MaximumLength(150);
        RuleFor(x => x.ExternalId).MaximumLength(150);
        RuleFor(x => x.SerialNumber).MaximumLength(150);
    }
}
