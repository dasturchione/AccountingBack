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

public sealed class CurrencyRevaluationPreviewDtoValidator : AbstractValidator<CurrencyRevaluationPreviewDto>
{
    public CurrencyRevaluationPreviewDtoValidator() => Include(new CurrencyRevaluationBaseDtoValidator());
}

public sealed class CurrencyRevaluationCreateDtoValidator : AbstractValidator<CurrencyRevaluationCreateDto>
{
    public CurrencyRevaluationCreateDtoValidator() => Include(new CurrencyRevaluationBaseDtoValidator());
}
