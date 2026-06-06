using FluentValidation;

namespace Application.Features.Roles;

public class RoleBaseDtoValidator : AbstractValidator<RoleBaseDto>
{
    public RoleBaseDtoValidator()
    {
        RuleFor(x => x.ShortName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(255);
    }
}
