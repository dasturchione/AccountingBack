using FluentValidation;

namespace Application.Features.Departments;

public class DepartmentCreateDtoValidator : AbstractValidator<DepartmentCreateDto>
{
    public DepartmentCreateDtoValidator()
    {
        Include(new DepartmentBaseDtoValidator());
    }
}
