using FluentValidation;
using SharedKernel.Constants;

namespace Application.Features.CashOperations;

public class CashOperationBaseDtoValidator : AbstractValidator<CashOperationBaseDto>
{
    public CashOperationBaseDtoValidator()
    {
        RuleFor(x => x.CashBoxId).GreaterThan(0);
        RuleFor(x => x.OperationTypeId).GreaterThan((short)0);
        RuleFor(x => x.DocNumber)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.DocNumber));
        RuleFor(x => x.CurrencyId).GreaterThan((short)0);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Comment).MaximumLength(1000).When(x => x.Comment != null);
        RuleFor(x => x.DestinationCashBoxId)
            .NotNull()
            .When(x => x.OperationTypeId == OperationTypeIdConst.TRANSFER)
            .WithMessage("Destination cash box is required for transfer operations.");
    }
}
