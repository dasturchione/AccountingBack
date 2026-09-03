using FluentValidation;

namespace Application.Features.Cmn.CurrencyRevaluations;

public sealed class CurrencyRevaluationBaseDtoValidator : AbstractValidator<CurrencyRevaluationBaseDto>
{
    public CurrencyRevaluationBaseDtoValidator()
    {
        RuleFor(x => x.RevaluationDate).NotEmpty();
        RuleFor(x => x.ProviderRateDate).LessThanOrEqualTo(x => x.RevaluationDate).When(x => x.ProviderRateDate.HasValue);
    }
}
