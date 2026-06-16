using FluentValidation;

namespace Application.Features.Acc.PostingRules;

public class PostingRuleLineCreateDtoValidator : AbstractValidator<PostingRuleLineCreateDto>
{
    public PostingRuleLineCreateDtoValidator()
    {
        Include(new PostingRuleLineBaseDtoValidator());
    }
}
