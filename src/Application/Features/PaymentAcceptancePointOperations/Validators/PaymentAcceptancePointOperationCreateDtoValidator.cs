using FluentValidation;

namespace Application.Features.PaymentAcceptancePointOperations;

public sealed class PaymentAcceptancePointOperationCreateDtoValidator : AbstractValidator<PaymentAcceptancePointOperationCreateDto>
{
    public PaymentAcceptancePointOperationCreateDtoValidator() =>
        Include(new PaymentAcceptancePointOperationBaseDtoValidator());
}
