using FluentValidation;

namespace Application.Features.Warehouses;

public sealed class WarehouseListFilterValidator : AbstractValidator<WarehouseListFilter>
{
    public WarehouseListFilterValidator()
    {
        RuleFor(x => x.BranchId)
            .GreaterThan(0)
            .When(x => x.BranchId.HasValue);
        RuleFor(x => x.Search)
            .MaximumLength(250)
            .When(x => x.Search != null);
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .When(x => x.PageSize.HasValue);
    }
}
