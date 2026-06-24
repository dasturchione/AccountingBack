using FluentValidation;

namespace Application.Features.Banks;

public class BankUpdateDtoValidator : AbstractValidator<BankUpdateDto>
{
    public BankUpdateDtoValidator()
    {
        Include(new BankBaseDtoValidator());
        RuleFor(x => x.StateId).GreaterThan((short)0);
    }
}
