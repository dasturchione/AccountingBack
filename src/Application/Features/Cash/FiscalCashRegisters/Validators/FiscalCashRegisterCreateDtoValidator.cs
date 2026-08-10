using FluentValidation;

namespace Application.Features.FiscalCashRegisters;

public class FiscalCashRegisterCreateDtoValidator : AbstractValidator<FiscalCashRegisterCreateDto>
{
    public FiscalCashRegisterCreateDtoValidator()
    {
        Include(new FiscalCashRegisterBaseDtoValidator());
    }
}
