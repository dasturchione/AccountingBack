using FluentValidation;

namespace Application.Features.Acc.PostingRules;

public class PostingRuleLineUpdateDtoValidator : AbstractValidator<PostingRuleLineUpdateDto>
{
    public PostingRuleLineUpdateDtoValidator()
    {
        Include(new PostingRuleLineBaseDtoValidator());
    }
}
