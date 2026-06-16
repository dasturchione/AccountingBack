using FluentValidation;

namespace Application.Features.Acc.PostingRules
{
    public class PostingRuleLineBaseDtoValidator : AbstractValidator<PostingRuleLineBaseDto>
    {
        public PostingRuleLineBaseDtoValidator()
        {
            RuleFor(x => x.SortOrder)
                .GreaterThan(0);

            RuleFor(x => x.AmountSource)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.QuantitySource)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.QuantitySource));

            RuleFor(x => x.ContentTemplate)
                .MaximumLength(500)
                .When(x => !string.IsNullOrWhiteSpace(x.ContentTemplate));

            RuleFor(x => x)
                .Must(x => x.DebitAccountId.HasValue || x.CreditAccountId.HasValue)
                .WithMessage("At least one of Debit Account or Credit Account must be specified."); ;
        }
    }
}
