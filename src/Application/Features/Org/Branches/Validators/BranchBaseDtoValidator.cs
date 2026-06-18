using FluentValidation;

namespace Application.Features.Branches;

public class BranchBaseDtoValidator : AbstractValidator<BranchBaseDto>
{
    public BranchBaseDtoValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Address).MaximumLength(500).When(x => x.Address != null);
        RuleFor(x => x.PhoneNumber).MaximumLength(50).When(x => x.PhoneNumber != null);
    }
}
