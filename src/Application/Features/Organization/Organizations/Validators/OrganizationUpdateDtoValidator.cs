using FluentValidation;

namespace Application.Features.Organizations;

public class OrganizationUpdateDtoValidator : AbstractValidator<OrganizationUpdateDto>
{
    public OrganizationUpdateDtoValidator()
    {
        Include(new OrganizationBaseDtoValidator());
        RuleFor(x => x.StateId).GreaterThan((short)0);
    }
}
