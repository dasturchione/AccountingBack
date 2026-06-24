using FluentValidation;

namespace Application.Features.Banks;

public class BankBaseDtoValidator : AbstractValidator<BankBaseDto>
{
    public BankBaseDtoValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Mfo).MaximumLength(20);
    }
}
