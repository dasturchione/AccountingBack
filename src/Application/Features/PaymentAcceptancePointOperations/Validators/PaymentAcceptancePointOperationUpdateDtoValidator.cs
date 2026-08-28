using FluentValidation;

namespace Application.Features.PaymentAcceptancePointOperations;

public sealed class PaymentAcceptancePointOperationUpdateDtoValidator : AbstractValidator<PaymentAcceptancePointOperationUpdateDto>
{
    public PaymentAcceptancePointOperationUpdateDtoValidator() =>
        Include(new PaymentAcceptancePointOperationBaseDtoValidator());
}
