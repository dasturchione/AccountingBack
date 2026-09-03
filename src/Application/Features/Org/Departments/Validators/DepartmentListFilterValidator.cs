using FluentValidation;

namespace Application.Features.Departments;

public sealed class DepartmentListFilterValidator : AbstractValidator<DepartmentListFilter>
{
    public DepartmentListFilterValidator()
    {
        RuleFor(filter => filter.BranchId)
            .GreaterThan(0)
            .When(filter => filter.BranchId.HasValue);
        RuleFor(filter => filter.Search)
            .MaximumLength(100)
            .When(filter => !string.IsNullOrWhiteSpace(filter.Search));
        RuleFor(filter => filter.Page).GreaterThan(0);
        RuleFor(filter => filter.PageSize)
            .GreaterThan(0)
            .When(filter => filter.PageSize.HasValue);
    }
}
