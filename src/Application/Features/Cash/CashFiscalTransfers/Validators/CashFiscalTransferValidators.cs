using FluentValidation;
using SharedKernel.Constants;

namespace Application.Features.CashFiscalTransfers;

public class CashFiscalTransferBaseDtoValidator : AbstractValidator<CashFiscalTransferBaseDto>
{
    public CashFiscalTransferBaseDtoValidator()
    {
        RuleFor(x => x.FiscalCashRegisterId).GreaterThan(0);
        RuleFor(x => x.CashBoxId).GreaterThan(0);
        RuleFor(x => x.DirectionId).Must(MovementDirectionIdConst.IsValid);
        RuleFor(x => x.DocDate).NotEmpty();
        RuleFor(x => x.CurrencyId).GreaterThan((short)0);
        RuleFor(x => x.Amount).GreaterThan(0m);
        RuleFor(x => x.ExchangeRate).GreaterThan(0m);
        RuleFor(x => x.FiscalCashAccountId).GreaterThan(0).When(x => x.FiscalCashAccountId.HasValue);
        RuleFor(x => x.CashBoxAccountId).GreaterThan(0).When(x => x.CashBoxAccountId.HasValue);
        RuleFor(x => x.Comment).MaximumLength(1000);
    }
}

public sealed class CashFiscalTransferCreateDtoValidator : AbstractValidator<CashFiscalTransferCreateDto>
{
    public CashFiscalTransferCreateDtoValidator() => Include(new CashFiscalTransferBaseDtoValidator());
}

public sealed class CashFiscalTransferUpdateDtoValidator : AbstractValidator<CashFiscalTransferUpdateDto>
{
    public CashFiscalTransferUpdateDtoValidator() => Include(new CashFiscalTransferBaseDtoValidator());
}
