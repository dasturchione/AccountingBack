using FluentValidation;

namespace Application.Features.Cmn.CurrencyRates;

public sealed class CurrencyRateImportRequestDtoValidator : AbstractValidator<CurrencyRateImportRequestDto>
{
    public CurrencyRateImportRequestDtoValidator()
    {
        RuleFor(x => x.ProviderCode).MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.ProviderCode));
        RuleFor(x => x.Date).LessThanOrEqualTo(DateTime.Today).When(x => x.Date.HasValue);
    }
}
