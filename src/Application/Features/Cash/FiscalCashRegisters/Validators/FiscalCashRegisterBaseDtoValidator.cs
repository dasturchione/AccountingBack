using FluentValidation;

namespace Application.Features.FiscalCashRegisters;

public class FiscalCashRegisterBaseDtoValidator : AbstractValidator<FiscalCashRegisterBaseDto>
{
    public FiscalCashRegisterBaseDtoValidator()
    {
        RuleFor(x => x.WarehouseId)
            .GreaterThan(0)
            .When(x => x.WarehouseId.HasValue);
        RuleFor(x => x.RegisterTypeId).GreaterThan((short)0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
        RuleFor(x => x.ExternalRegisterId).MaximumLength(100);
        RuleFor(x => x.Model).MaximumLength(100);
        RuleFor(x => x.SerialNumber).MaximumLength(100);
        RuleFor(x => x.FiscalModuleNumber).MaximumLength(100);
    }
}
