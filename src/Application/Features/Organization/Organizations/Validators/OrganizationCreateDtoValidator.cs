using FluentValidation;

namespace Application.Features.Organizations;

public class OrganizationCreateDtoValidator : AbstractValidator<OrganizationCreateDto>
{
    public OrganizationCreateDtoValidator()
    {
        Include(new OrganizationBaseDtoValidator());
    }
}
