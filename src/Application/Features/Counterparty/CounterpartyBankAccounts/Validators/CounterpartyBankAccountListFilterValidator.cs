using FluentValidation;

namespace Application.Features.CounterpartyBankAccounts;

public sealed class CounterpartyBankAccountListFilterValidator
    : AbstractValidator<CounterpartyBankAccountListFilter>
{
    public CounterpartyBankAccountListFilterValidator()
    {
        RuleFor(filter => filter.CounterpartyId)
            .GreaterThan(0)
            .When(filter => filter.CounterpartyId.HasValue);
        RuleFor(filter => filter.Search)
            .MaximumLength(50)
            .When(filter => !string.IsNullOrWhiteSpace(filter.Search));
        RuleFor(filter => filter.Page).GreaterThan(0);
        RuleFor(filter => filter.PageSize)
            .GreaterThan(0)
            .When(filter => filter.PageSize.HasValue);
    }
}
