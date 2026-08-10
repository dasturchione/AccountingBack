using FluentValidation;

namespace Application.Features.BankTerminals;

public class BankTerminalUpdateDtoValidator : AbstractValidator<BankTerminalUpdateDto>
{
    public BankTerminalUpdateDtoValidator()
    {
        Include(new BankTerminalBaseDtoValidator());
        RuleFor(x => x.StateId).GreaterThan((short)0);
    }
}
