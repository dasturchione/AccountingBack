using FluentValidation;

namespace Application.Features.Roles;

public class RoleCreateDtoValidator : AbstractValidator<RoleCreateDto>
{
    public RoleCreateDtoValidator()
    {
        Include(new RoleBaseDtoValidator());
    }
}
