using FluentValidation;

namespace Application.Features.FiscalCashRegisters;

public class FiscalCashRegisterUpdateDtoValidator : AbstractValidator<FiscalCashRegisterUpdateDto>
{
    public FiscalCashRegisterUpdateDtoValidator()
    {
        Include(new FiscalCashRegisterBaseDtoValidator());
        RuleFor(x => x.StateId).GreaterThan((short)0);
    }
}
