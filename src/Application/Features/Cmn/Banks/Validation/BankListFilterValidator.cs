using FluentValidation;

namespace Application.Features.Banks;

public sealed class BankListFilterValidator : AbstractValidator<BankListFilter>
{
    public BankListFilterValidator()
    {
        RuleFor(filter => filter.StateId)
            .GreaterThan((short)0)
            .When(filter => filter.StateId.HasValue);
        RuleFor(filter => filter.Search)
            .MaximumLength(100)
            .When(filter => !string.IsNullOrWhiteSpace(filter.Search));
        RuleFor(filter => filter.Page).GreaterThan(0);
        RuleFor(filter => filter.PageSize)
            .GreaterThan(0)
            .When(filter => filter.PageSize.HasValue);
    }
}
