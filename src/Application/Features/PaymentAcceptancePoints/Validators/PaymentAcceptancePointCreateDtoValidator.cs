using FluentValidation;

namespace Application.Features.PaymentAcceptancePoints;

public class PaymentAcceptancePointCreateDtoValidator : AbstractValidator<PaymentAcceptancePointCreateDto>
{
    public PaymentAcceptancePointCreateDtoValidator()
    {
        Include(new PaymentAcceptancePointBaseDtoValidator());
    }
}
