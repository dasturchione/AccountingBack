using FluentValidation;

namespace Application.Features.CashOperations;

public class CashOperationBaseDtoValidator : AbstractValidator<CashOperationBaseDto>
{
    public CashOperationBaseDtoValidator()
    {
        RuleFor(x => x.OrganizationId).GreaterThan(0);
        RuleFor(x => x.CashBoxId).GreaterThan(0);
        RuleFor(x => x.OperationTypeId).GreaterThan((short)0);
        RuleFor(x => x.DocNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.CurrencyId).GreaterThan((short)0);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Comment).MaximumLength(1000).When(x => x.Comment != null);
    }
}
