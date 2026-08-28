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
        RuleFor(x => x.BankDocumentNumber).MaximumLength(150).When(x => x.BankDocumentNumber != null);
        RuleFor(x => x.ClassificationCategoryId).GreaterThan((short)0).When(x => x.ClassificationCategoryId.HasValue);
        RuleFor(x => x.ClassificationRuleId).GreaterThan(0).When(x => x.ClassificationRuleId.HasValue);
        RuleFor(x => x.RelatedDocumentId).GreaterThan(0).When(x => x.RelatedDocumentId.HasValue);
        RuleFor(x => x.ClassificationCategoryId).NotNull().When(x => x.ClassificationRuleId.HasValue);
        RuleFor(x => x.Comment).MaximumLength(1000).When(x => x.Comment != null);
    }
}
