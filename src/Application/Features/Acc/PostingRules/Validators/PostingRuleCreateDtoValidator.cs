using FluentValidation;

namespace Application.Features.Acc.PostingRules;

public class PostingRuleCreateDtoValidator : AbstractValidator<PostingRuleCreateDto>
{
    public PostingRuleCreateDtoValidator()
    {
        Include(new PostingRuleBaseDtoValidator());

        RuleFor(x => x.DocumentTypeId).GreaterThan((short)0);

        RuleFor(x => x.Lines)
            .NotNull()
            .NotEmpty()
            .WithMessage("At least one posting rule line is required.");

        RuleForEach(x => x.Lines)
            .SetValidator(new PostingRuleLineCreateDtoValidator());
    }
}
