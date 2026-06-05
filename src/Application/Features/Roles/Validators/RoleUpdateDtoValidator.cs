using FluentValidation;

namespace Application.Features.Roles;

public class RoleUpdateDtoValidator : AbstractValidator<RoleUpdateDto>
{
    public RoleUpdateDtoValidator()
    {
        Include(new RoleBaseDtoValidator());
        RuleFor(x => x.StateId).GreaterThan((short)0);
    }
}
