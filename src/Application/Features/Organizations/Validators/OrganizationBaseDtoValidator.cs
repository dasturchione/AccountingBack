using FluentValidation;

namespace Application.Features.Organizations;

public class OrganizationBaseDtoValidator : AbstractValidator<OrganizationBaseDto>
{
    public OrganizationBaseDtoValidator()
    {
        RuleFor(x => x.ShortName).NotEmpty().MaximumLength(250);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Inn).NotEmpty().MaximumLength(20);
        RuleFor(x => x.PhoneNumber).MaximumLength(50).When(x => x.PhoneNumber != null);
        RuleFor(x => x.RegionId).GreaterThan(0);
        RuleFor(x => x.Address).MaximumLength(1000).When(x => x.Address != null);
        RuleFor(x => x.Director).MaximumLength(250).When(x => x.Director != null);
    }
}
