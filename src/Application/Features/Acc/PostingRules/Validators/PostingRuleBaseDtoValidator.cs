using FluentValidation;

namespace Application.Features.Acc.PostingRules;

public class PostingRuleBaseDtoValidator : AbstractValidator<PostingRuleBaseDto>
{
    public PostingRuleBaseDtoValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
    }
}
