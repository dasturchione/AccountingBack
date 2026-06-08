using FluentValidation;

namespace Application.Features.BankOperations;

public class BankOperationUpdateDtoValidator : AbstractValidator<BankOperationUpdateDto>
{
    public BankOperationUpdateDtoValidator()
    {
        Include(new BankOperationBaseDtoValidator());
        RuleFor(x => x.StateId).GreaterThan((short)0);
    }
}
