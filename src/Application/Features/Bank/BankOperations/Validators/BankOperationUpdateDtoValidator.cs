using FluentValidation;

namespace Application.Features.BankOperations;

public class BankOperationUpdateDtoValidator : AbstractValidator<BankOperationUpdateDto>
{
    public BankOperationUpdateDtoValidator()
    {
        Include(new BankOperationBaseDtoValidator());
        RuleFor(x => x.DocNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.StatusId).GreaterThan((short)0);
        RuleFor(x => x.StateId).GreaterThan((short)0);
    }
}
