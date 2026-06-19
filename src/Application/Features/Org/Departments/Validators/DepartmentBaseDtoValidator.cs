using FluentValidation;

namespace Application.Features.Departments;

public class DepartmentBaseDtoValidator : AbstractValidator<DepartmentBaseDto>
{
    public DepartmentBaseDtoValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
    }
}
