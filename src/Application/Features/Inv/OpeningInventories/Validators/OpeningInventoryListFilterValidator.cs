using FluentValidation;

namespace Application.Features.Inv.OpeningInventories;

public sealed class OpeningInventoryListFilterValidator
    : AbstractValidator<OpeningInventoryListFilter>
{
    public OpeningInventoryListFilterValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 200).When(x => x.PageSize.HasValue);
        RuleFor(x => x.Search).MaximumLength(250).When(x => x.Search != null);
        RuleFor(x => x.SortBy).MaximumLength(50).When(x => x.SortBy != null);
        RuleFor(x => x.DateTo)
            .GreaterThanOrEqualTo(x => x.DateFrom)
            .When(x => x.DateFrom.HasValue && x.DateTo.HasValue);
    }
}
