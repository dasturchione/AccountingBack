using FluentValidation;

namespace Application.Features.BankOperations;

public class BankOperationCreateDtoValidator : AbstractValidator<BankOperationCreateDto>
{
    public BankOperationCreateDtoValidator()
    {
        Include(new BankOperationBaseDtoValidator());

        RuleFor(x => x.PaymentPurposeId).GreaterThan((short)0);
    }
}
