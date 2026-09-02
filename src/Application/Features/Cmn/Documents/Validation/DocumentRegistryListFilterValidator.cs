using FluentValidation;

namespace Application.Features.Cmn.Documents;

public sealed class DocumentRegistryListFilterValidator : AbstractValidator<DocumentRegistryListFilter>
{
    public DocumentRegistryListFilterValidator()
    {
        RuleFor(filter => filter.DocumentTypeCode)
            .MaximumLength(50)
            .When(filter => !string.IsNullOrWhiteSpace(filter.DocumentTypeCode));
        RuleFor(filter => filter.CurrencyId)
            .GreaterThan((short)0)
            .When(filter => filter.CurrencyId.HasValue);
        RuleFor(filter => filter.StatusId)
            .GreaterThan((short)0)
            .When(filter => filter.StatusId.HasValue);
        RuleFor(filter => filter.StateId)
            .GreaterThan((short)0)
            .When(filter => filter.StateId.HasValue);
        RuleFor(filter => filter.Search)
            .MaximumLength(100)
            .When(filter => !string.IsNullOrWhiteSpace(filter.Search));
    }
}
