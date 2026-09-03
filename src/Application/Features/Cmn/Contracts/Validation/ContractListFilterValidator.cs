using FluentValidation;

namespace Application.Features.Contracts;

public sealed class ContractListFilterValidator : AbstractValidator<ContractListFilter>
{
    public ContractListFilterValidator()
    {
        RuleFor(filter => filter.CounterpartyId)
            .GreaterThan(0)
            .When(filter => filter.CounterpartyId.HasValue);
        RuleFor(filter => filter.ContractTypeId)
            .GreaterThan((short)0)
            .When(filter => filter.ContractTypeId.HasValue);
        RuleFor(filter => filter.Search)
            .MaximumLength(100)
            .When(filter => !string.IsNullOrWhiteSpace(filter.Search));
        RuleFor(filter => filter.Page).GreaterThan(0);
        RuleFor(filter => filter.PageSize)
            .GreaterThan(0)
            .When(filter => filter.PageSize.HasValue);
    }
}
