using FluentValidation;

namespace Application.Features.ChartAccounts;

public class ChartAccountBaseDtoValidator : AbstractValidator<ChartAccountBaseDto>
{
    public ChartAccountBaseDtoValidator()
    {
        RuleFor(x => x.OrganizationId).GreaterThan(0);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
    }
}
