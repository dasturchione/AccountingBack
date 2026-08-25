using FluentValidation;
using SharedKernel.Constants;

namespace Application.Features.BankOperations;

public class BankOperationBaseDtoValidator : AbstractValidator<BankOperationBaseDto>
{
    public BankOperationBaseDtoValidator()
    {
        RuleFor(x => x.BankAccountId).GreaterThan(0);
        RuleFor(x => x.DirectionId).Must(MovementDirectionIdConst.IsValid);
        RuleFor(x => x.CurrencyId).GreaterThan((short)0);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Comment).MaximumLength(1000).When(x => x.Comment != null);
    }
}
