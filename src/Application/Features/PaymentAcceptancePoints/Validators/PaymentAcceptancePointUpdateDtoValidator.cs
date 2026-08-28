using FluentValidation;

namespace Application.Features.PaymentAcceptancePoints;

public class PaymentAcceptancePointUpdateDtoValidator : AbstractValidator<PaymentAcceptancePointUpdateDto>
{
    public PaymentAcceptancePointUpdateDtoValidator()
    {
        Include(new PaymentAcceptancePointBaseDtoValidator());
        RuleFor(x => x.StateId).GreaterThan((short)0);
    }
}
