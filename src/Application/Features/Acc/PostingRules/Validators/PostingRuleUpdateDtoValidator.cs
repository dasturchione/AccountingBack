using FluentValidation;

namespace Application.Features.Acc.PostingRules;

public class PostingRuleUpdateDtoValidator : AbstractValidator<PostingRuleUpdateDto>
{
    public PostingRuleUpdateDtoValidator()
    {
        Include(new PostingRuleBaseDtoValidator());

        RuleFor(x => x.StateId).NotEmpty();

        RuleFor(x => x.Lines)
            .NotNull()
            .NotEmpty()
            .WithMessage("At least one posting rule line is required.");

        RuleForEach(x => x.Lines)
            .SetValidator(new PostingRuleLineUpdateDtoValidator());
    }
}
