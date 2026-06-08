using FluentValidation;

namespace Application.Features.Departments;

public class DepartmentUpdateDtoValidator : AbstractValidator<DepartmentUpdateDto>
{
    public DepartmentUpdateDtoValidator()
    {
        Include(new DepartmentBaseDtoValidator());
        RuleFor(x => x.StateId).GreaterThan((short)0);
    }
}
